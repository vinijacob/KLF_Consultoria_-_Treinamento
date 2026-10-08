using Klf.Application.DTOs.Auth;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Services.Auth;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class TwoFactorAuthServiceTests
{
    private static readonly UserAccount User = new(Guid.CreateVersion7(), "admin@klf.test", "Admin", ["Admin"]);
    private static readonly LoginRequest Login = new("admin@klf.test", "x");

    private readonly InMemoryRefreshTokenRepository _refreshTokens = new();
    private readonly MutableTimeProvider _clock = new(new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeTokenService _tokens;
    private readonly FakeIdentityService _identity;

    public TwoFactorAuthServiceTests()
    {
        _tokens = new FakeTokenService(_clock);
        _identity = new FakeIdentityService(User);
    }

    [Fact]
    public async Task Login_returns_setup_challenge_and_no_session_when_two_factor_is_required_and_not_enrolled()
    {
        var result = await CreateService(required: true).LoginAsync(Login, TestContext.Current.CancellationToken);

        Assert.Null(result.Session);
        Assert.True(result.Challenge!.SetupRequired);
        Assert.Empty(_refreshTokens.Tokens);
    }

    [Fact]
    public async Task Login_returns_verify_challenge_when_user_already_has_two_factor_even_if_not_required()
    {
        _identity.TwoFactorEnabled = true;

        var result = await CreateService(required: false).LoginAsync(Login, TestContext.Current.CancellationToken);

        Assert.Null(result.Session);
        Assert.False(result.Challenge!.SetupRequired);
    }

    [Fact]
    public async Task Login_starts_session_when_two_factor_is_not_required_and_not_enrolled()
    {
        var result = await CreateService(required: false).LoginAsync(Login, TestContext.Current.CancellationToken);

        Assert.NotNull(result.Session);
        Assert.Null(result.Challenge);
    }

    [Fact]
    public async Task Verify_starts_session_when_code_is_valid()
    {
        _identity.TwoFactorEnabled = true;
        var service = CreateService(required: true);
        var challenge = (await service.LoginAsync(Login, TestContext.Current.CancellationToken)).Challenge!;

        var session = await service.VerifyTwoFactorAsync(new TwoFactorCodeRequest(challenge.Token, FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);

        Assert.Equal($"access-for-{User.Id}", session.AccessToken);
        Assert.Single(_refreshTokens.Tokens);
    }

    [Fact]
    public async Task Verify_throws_unauthorized_when_code_is_wrong_and_locks_after_five_attempts()
    {
        _identity.TwoFactorEnabled = true;
        var service = CreateService(required: true);
        var challenge = (await service.LoginAsync(Login, TestContext.Current.CancellationToken)).Challenge!;
        var token = TestContext.Current.CancellationToken;

        var wrong = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.VerifyTwoFactorAsync(new TwoFactorCodeRequest(challenge.Token, "000000"), token));
        for (var i = 0; i < 3; i++)
        {
            await Assert.ThrowsAsync<UnauthorizedException>(() => service.VerifyTwoFactorAsync(new TwoFactorCodeRequest(challenge.Token, "000000"), token));
        }

        var locked = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.VerifyTwoFactorAsync(new TwoFactorCodeRequest(challenge.Token, "000000"), token));
        var lockedEvenWithRightCode = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.VerifyTwoFactorAsync(new TwoFactorCodeRequest(challenge.Token, FakeIdentityService.ValidCode), token));

        Assert.Equal("Código inválido.", wrong.Message);
        Assert.Contains("bloqueada", locked.Message, StringComparison.Ordinal);
        Assert.Contains("bloqueada", lockedEvenWithRightCode.Message, StringComparison.Ordinal);
        Assert.Empty(_refreshTokens.Tokens);
    }

    [Fact]
    public async Task Verify_throws_unauthorized_when_token_is_invalid_or_user_has_no_two_factor()
    {
        var service = CreateService(required: true);
        var token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.VerifyTwoFactorAsync(new TwoFactorCodeRequest("lixo", FakeIdentityService.ValidCode), token));
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.VerifyTwoFactorAsync(new TwoFactorCodeRequest(_tokens.CreateTwoFactorChallenge(User.Id), FakeIdentityService.ValidCode), token));
    }

    [Fact]
    public async Task Recover_starts_session_once_and_rejects_the_same_code_again()
    {
        _identity.TwoFactorEnabled = true;
        var service = CreateService(required: true);
        var challenge = (await service.LoginAsync(Login, TestContext.Current.CancellationToken)).Challenge!;
        var request = new TwoFactorRecoveryRequest(challenge.Token, FakeIdentityService.ValidRecoveryCode);

        var session = await service.RecoverWithCodeAsync(request, TestContext.Current.CancellationToken);

        Assert.NotNull(session);
        await Assert.ThrowsAsync<UnauthorizedException>(() => service.RecoverWithCodeAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Setup_returns_key_and_uri_when_user_is_not_enrolled()
    {
        var service = CreateService(required: true);
        var challenge = (await service.LoginAsync(Login, TestContext.Current.CancellationToken)).Challenge!;

        var setup = await service.StartTwoFactorSetupAsync(new TwoFactorSetupRequest(challenge.Token), TestContext.Current.CancellationToken);

        Assert.StartsWith("otpauth://totp/", setup.OtpAuthUri, StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(setup.SharedKey));
    }

    [Fact]
    public async Task Setup_and_enable_throw_conflict_when_two_factor_is_already_enabled()
    {
        _identity.TwoFactorEnabled = true;
        var service = CreateService(required: true);
        var challenge = (await service.LoginAsync(Login, TestContext.Current.CancellationToken)).Challenge!;
        var token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ConflictException>(() => service.StartTwoFactorSetupAsync(new TwoFactorSetupRequest(challenge.Token), token));
        await Assert.ThrowsAsync<ConflictException>(() => service.EnableTwoFactorAsync(new TwoFactorCodeRequest(challenge.Token, FakeIdentityService.ValidCode), token));
    }

    [Fact]
    public async Task Enable_turns_two_factor_on_starts_session_and_returns_ten_recovery_codes()
    {
        var service = CreateService(required: true);
        var challenge = (await service.LoginAsync(Login, TestContext.Current.CancellationToken)).Challenge!;

        var result = await service.EnableTwoFactorAsync(new TwoFactorCodeRequest(challenge.Token, FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);

        Assert.True(_identity.TwoFactorEnabled);
        Assert.Equal(10, result.RecoveryCodes.Count);
        Assert.Single(_refreshTokens.Tokens);
        Assert.NotNull(result.Session);
    }

    [Fact]
    public async Task Enable_throws_unauthorized_and_keeps_two_factor_off_when_code_is_wrong()
    {
        var service = CreateService(required: true);
        var challenge = (await service.LoginAsync(Login, TestContext.Current.CancellationToken)).Challenge!;

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.EnableTwoFactorAsync(new TwoFactorCodeRequest(challenge.Token, "000000"), TestContext.Current.CancellationToken));

        Assert.False(_identity.TwoFactorEnabled);
        Assert.Empty(_refreshTokens.Tokens);
    }

    [Fact]
    public async Task Regenerate_returns_new_codes_when_code_is_valid_and_two_factor_is_enabled()
    {
        _identity.TwoFactorEnabled = true;
        var service = CreateService(required: true);

        var codes = await service.RegenerateRecoveryCodesAsync(User.Id, new RegenerateRecoveryCodesRequest(FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);

        Assert.Equal(10, codes.Count);
        Assert.DoesNotContain(FakeIdentityService.ValidRecoveryCode, _identity.RecoveryCodes);
    }

    [Fact]
    public async Task Regenerate_throws_conflict_when_two_factor_is_off_and_unauthorized_when_code_is_wrong()
    {
        var service = CreateService(required: true);
        var token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegenerateRecoveryCodesAsync(User.Id, new RegenerateRecoveryCodesRequest(FakeIdentityService.ValidCode), token));

        _identity.TwoFactorEnabled = true;
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RegenerateRecoveryCodesAsync(User.Id, new RegenerateRecoveryCodesRequest("000000"), token));
    }

    [Fact]
    public async Task Current_user_reports_two_factor_status()
    {
        _identity.TwoFactorEnabled = true;

        var me = await CreateService(required: true).GetCurrentUserAsync(User.Id, TestContext.Current.CancellationToken);

        Assert.True(me.TwoFactorEnabled);
    }

    private AuthService CreateService(bool required) =>
        new(_identity, _tokens, new FakeTwoFactorPolicy(required), new FakeEmailSender(), new FakeFrontendLinks(), _refreshTokens, _refreshTokens, _clock);
}
