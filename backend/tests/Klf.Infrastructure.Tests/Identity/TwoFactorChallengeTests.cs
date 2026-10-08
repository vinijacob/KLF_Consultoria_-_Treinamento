using Klf.Application.Interfaces.Identity;
using Klf.Infrastructure.Identity;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Klf.Infrastructure.Tests.Identity;

public sealed class TwoFactorChallengeTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "klf-api",
        Audience = "klf-web",
        SigningKey = "test-signing-key-that-is-long-enough-for-hmac-sha256",
    };

    private readonly Clock _clock = new(new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero));
    private readonly TokenService _service;

    public TwoFactorChallengeTests()
    {
        _service = new TokenService(Microsoft.Extensions.Options.Options.Create(Options), _clock);
    }

    [Fact]
    public async Task Challenge_returns_the_user_id_when_token_is_fresh()
    {
        var userId = Guid.CreateVersion7();

        var token = _service.CreateTwoFactorChallenge(userId);

        Assert.Equal(userId, await _service.ReadTwoFactorChallengeAsync(token));
    }

    [Fact]
    public async Task Challenge_is_rejected_after_five_minutes()
    {
        var token = _service.CreateTwoFactorChallenge(Guid.CreateVersion7());

        _clock.Now = _clock.Now.AddMinutes(5).AddSeconds(1);

        Assert.Null(await _service.ReadTwoFactorChallengeAsync(token));
    }

    [Fact]
    public async Task Challenge_is_rejected_when_token_is_garbage_or_signed_with_another_key()
    {
        var other = new TokenService(
            Microsoft.Extensions.Options.Options.Create(new JwtOptions { Issuer = "klf-api", Audience = "klf-web", SigningKey = "another-signing-key-that-is-long-enough-123456" }),
            _clock);

        Assert.Null(await _service.ReadTwoFactorChallengeAsync("lixo"));
        Assert.Null(await _service.ReadTwoFactorChallengeAsync(other.CreateTwoFactorChallenge(Guid.CreateVersion7())));
    }

    [Fact]
    public async Task Access_token_is_not_accepted_as_a_challenge_and_challenge_is_not_valid_as_access_token()
    {
        var user = new UserAccount(Guid.CreateVersion7(), "a@klf.test", "A", ["Admin"]);
        var access = _service.Generate(user).Token;
        var challenge = _service.CreateTwoFactorChallenge(user.Id);

        var asAccess = await new JsonWebTokenHandler().ValidateTokenAsync(challenge, new TokenValidationParameters
        {
            ValidIssuer = Options.Issuer,
            ValidAudience = Options.Audience,
            IssuerSigningKey = Options.CreateSigningKey(),
        });

        Assert.Null(await _service.ReadTwoFactorChallengeAsync(access));
        Assert.False(asAccess.IsValid);
    }

    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }
}
