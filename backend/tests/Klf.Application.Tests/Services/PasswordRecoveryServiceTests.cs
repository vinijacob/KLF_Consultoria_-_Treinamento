using Klf.Application.DTOs.Auth;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Services.Auth;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class PasswordRecoveryServiceTests
{
    private static readonly UserAccount User = new(Guid.CreateVersion7(), "admin@klf.test", "Kilciene <b>", ["Admin"]);

    private readonly InMemoryRefreshTokenRepository _refreshTokens = new();
    private readonly MutableTimeProvider _clock = new(new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeIdentityService _identity = new(User);
    private readonly FakeEmailSender _email = new();
    private readonly AuthService _service;

    public PasswordRecoveryServiceTests()
    {
        _service = new AuthService(
            _identity,
            new FakeTokenService(_clock),
            new FakeTwoFactorPolicy(false),
            _email,
            new FakeFrontendLinks(),
            _refreshTokens,
            _refreshTokens,
            _clock);
    }

    [Fact]
    public async Task Forgot_sends_one_email_with_reset_link_and_escaped_name_when_account_exists()
    {
        await _service.ForgotPasswordAsync(new ForgotPasswordRequest(" admin@klf.test "), TestContext.Current.CancellationToken);

        var message = Assert.Single(_email.Sent);
        Assert.Equal("admin@klf.test", message.To);
        Assert.Contains("https://klf.test/painel/redefinir-senha?email=admin%40klf.test&amp;token=reset-token", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("30 minutos", message.TextBody, StringComparison.Ordinal);
        Assert.DoesNotContain("<b>", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Kilciene &lt;b&gt;", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Forgot_sends_nothing_and_does_not_fail_when_account_does_not_exist()
    {
        await _service.ForgotPasswordAsync(new ForgotPasswordRequest("ninguem@klf.test"), TestContext.Current.CancellationToken);

        Assert.Empty(_email.Sent);
    }

    [Fact]
    public async Task Reset_changes_password_revokes_every_session_and_sends_notice()
    {
        var login = (await CreateLoggedInSessionsAsync()).ToList();

        await _service.ResetPasswordAsync(Reset("Nova@Senha1234"), TestContext.Current.CancellationToken);

        Assert.Equal("Nova@Senha1234", _identity.PasswordAfterReset);
        Assert.All(_refreshTokens.Tokens, t => Assert.False(t.IsActive(_clock.Now.UtcDateTime)));
        Assert.NotEmpty(login);
        var notice = Assert.Single(_email.Sent);
        Assert.Contains("alterada", notice.Subject, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Reset_throws_validation_when_token_was_already_used()
    {
        await _service.ResetPasswordAsync(Reset("Nova@Senha1234"), TestContext.Current.CancellationToken);

        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ResetPasswordAsync(Reset("Outra@Senha5678"), TestContext.Current.CancellationToken));

        Assert.Equal(["Link inválido ou expirado. Solicite um novo."], error.Errors["Token"]);
    }

    [Fact]
    public async Task Reset_throws_validation_when_token_or_email_is_wrong_without_changing_anything()
    {
        var token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ResetPasswordAsync(new ResetPasswordRequest("admin@klf.test", "errado", "Nova@Senha1234"), token));
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ResetPasswordAsync(new ResetPasswordRequest("outro@klf.test", FakeIdentityService.ValidResetToken, "Nova@Senha1234"), token));

        Assert.Null(_identity.PasswordAfterReset);
        Assert.Empty(_email.Sent);
    }

    [Fact]
    public async Task Reset_throws_validation_with_policy_messages_and_keeps_token_valid_when_password_is_weak()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ResetPasswordAsync(Reset("curta"), TestContext.Current.CancellationToken));

        Assert.Equal(["A senha deve ter pelo menos 10 caracteres."], error.Errors["NewPassword"]);
        Assert.False(_identity.ResetTokenUsed);
        Assert.Empty(_email.Sent);
    }

    private static ResetPasswordRequest Reset(string password) =>
        new("admin@klf.test", FakeIdentityService.ValidResetToken, password);

    private async Task<IEnumerable<RefreshToken>> CreateLoggedInSessionsAsync()
    {
        var login = new LoginRequest("admin@klf.test", "x");
        await _service.LoginAsync(login, TestContext.Current.CancellationToken);
        await _service.LoginAsync(login, TestContext.Current.CancellationToken);

        return _refreshTokens.Tokens;
    }
}
