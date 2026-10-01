using Klf.Application.Interfaces.Identity;
using Klf.Infrastructure.Identity;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Klf.Infrastructure.Tests.Identity;

public sealed class TokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "klf-api",
        Audience = "klf-web",
        SigningKey = "test-signing-key-that-is-long-enough-for-hmac-sha256",
        AccessTokenMinutes = 30,
    };

    private static readonly UserAccount User = new(Guid.CreateVersion7(), "admin@klf.test", "Admin Teste", ["Admin", "Editor"]);

    [Fact]
    public async Task Token_is_valid_and_carries_user_claims_when_generated()
    {
        var token = new TokenService(Microsoft.Extensions.Options.Options.Create(Options), TimeProvider.System).Generate(User);

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, ValidationParameters(Options.CreateSigningKey()));

        Assert.True(result.IsValid);
        Assert.Equal(User.Id.ToString(), result.Claims["sub"]);
        Assert.Equal(User.Email, result.Claims["email"]);
        Assert.Equal(["Admin", "Editor"], ((IEnumerable<object>)result.Claims["role"]).Select(r => r.ToString()));
    }

    [Fact]
    public void Expiration_uses_configured_lifetime_when_token_is_generated()
    {
        var before = DateTime.UtcNow;

        var token = new TokenService(Microsoft.Extensions.Options.Options.Create(Options), TimeProvider.System).Generate(User);

        Assert.InRange(token.ExpiresAt, before.AddMinutes(30), DateTime.UtcNow.AddMinutes(30));
    }

    [Fact]
    public async Task Token_is_rejected_when_signed_with_another_key()
    {
        var token = new TokenService(Microsoft.Extensions.Options.Options.Create(Options), TimeProvider.System).Generate(User);
        var otherKey = new SymmetricSecurityKey("another-signing-key-that-is-also-long-enough"u8.ToArray());

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, ValidationParameters(otherKey));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Refresh_tokens_are_unique_and_long_when_generated()
    {
        var service = new TokenService(Microsoft.Extensions.Options.Options.Create(Options), TimeProvider.System);

        var first = service.CreateRefreshToken();
        var second = service.CreateRefreshToken();

        Assert.NotEqual(first.Token, second.Token);
        Assert.True(first.Token.Length >= 80);
    }

    [Fact]
    public void Refresh_token_hash_is_sha256_hex_and_matches_stored_hash_when_recomputed()
    {
        var service = new TokenService(Microsoft.Extensions.Options.Options.Create(Options), TimeProvider.System);
        var token = service.CreateRefreshToken();

        Assert.Equal(64, token.Hash.Length);
        Assert.Equal(token.Hash, service.HashRefreshToken(token.Token));
        Assert.NotEqual(token.Token, token.Hash);
    }

    private static TokenValidationParameters ValidationParameters(SecurityKey key) => new()
    {
        ValidIssuer = Options.Issuer,
        ValidAudience = Options.Audience,
        IssuerSigningKey = key,
    };
}
