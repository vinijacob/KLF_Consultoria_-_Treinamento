using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

using Klf.Application.Interfaces.Identity;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Klf.Infrastructure.Identity;

internal sealed class TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    private const string PurposeClaim = "purpose";
    private const string TwoFactorPurpose = "2fa";
    private static readonly TimeSpan TwoFactorChallengeLifetime = TimeSpan.FromMinutes(5);

    private readonly JsonWebTokenHandler _handler = new();

    private static string TwoFactorAudience(JwtOptions jwt) => $"{jwt.Audience}:2fa";

    public AccessToken Generate(UserAccount user)
    {
        var jwt = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddMinutes(jwt.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(jwt.CreateSigningKey(), SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [JwtRegisteredClaimNames.Name] = user.FullName,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                [ClaimTypeNames.Role] = user.Roles.ToArray(),
            },
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }

    public NewRefreshToken CreateRefreshToken()
    {
        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));
        var expiresAt = timeProvider.GetUtcNow().UtcDateTime.AddHours(options.Value.RefreshTokenHours);

        return new NewRefreshToken(token, HashRefreshToken(token), expiresAt);
    }

    public string CreateTwoFactorChallenge(Guid userId)
    {
        var jwt = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = TwoFactorAudience(jwt),
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(TwoFactorChallengeLifetime),
            SigningCredentials = new SigningCredentials(jwt.CreateSigningKey(), SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = userId.ToString(),
                [PurposeClaim] = TwoFactorPurpose,
            },
        });
    }

    public async Task<Guid?> ReadTwoFactorChallengeAsync(string token)
    {
        var jwt = options.Value;

        var result = await _handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = TwoFactorAudience(jwt),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.CreateSigningKey(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            LifetimeValidator = (notBefore, expires, _, _) =>
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;

                return (notBefore is null || notBefore <= now) && expires > now;
            },
        });

        if (!result.IsValid
            || !result.Claims.TryGetValue(PurposeClaim, out var purpose)
            || purpose as string != TwoFactorPurpose
            || !result.Claims.TryGetValue(JwtRegisteredClaimNames.Sub, out var subject)
            || !Guid.TryParse(subject as string, out var userId))
        {
            return null;
        }

        return userId;
    }

    public string HashRefreshToken(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
