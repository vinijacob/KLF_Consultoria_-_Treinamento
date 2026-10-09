using Klf.Application.DTOs.Auth;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Services.Auth;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class AuthServiceTests
{
    private static readonly UserAccount User = new(Guid.CreateVersion7(), "admin@klf.test", "Admin", ["Admin"]);
    private static readonly LoginRequest ValidLogin = new(" admin@klf.test ", "x");

    private readonly InMemoryRefreshTokenRepository _refreshTokens = new();
    private readonly MutableTimeProvider _clock = new(new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeTokenService _tokens;

    public AuthServiceTests()
    {
        _tokens = new FakeTokenService(_clock);
    }

    [Fact]
    public async Task Login_starts_session_with_stored_refresh_token_when_credentials_are_valid()
    {
        var session = await LoginSessionAsync(CreateService(CredentialsCheckResult.Success(User)));

        var stored = Assert.Single(_refreshTokens.Tokens);
        Assert.Equal($"access-for-{User.Id}", session.AccessToken);
        Assert.Equal(Hash(session.RefreshToken), stored.TokenHash);
        Assert.NotEqual(session.RefreshToken, stored.TokenHash);
        Assert.Equal(_clock.Now.UtcDateTime.AddHours(8), session.SessionExpiresAt);
    }

    [Fact]
    public async Task Login_throws_unauthorized_when_credentials_are_invalid()
    {
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CreateService(CredentialsCheckResult.Invalid).LoginAsync(ValidLogin, TestContext.Current.CancellationToken));

        Assert.Equal("E-mail ou senha inválidos.", exception.Message);
        Assert.Empty(_refreshTokens.Tokens);
    }

    [Fact]
    public async Task Login_throws_unauthorized_with_lockout_message_when_account_is_locked()
    {
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CreateService(CredentialsCheckResult.LockedOut).LoginAsync(ValidLogin, TestContext.Current.CancellationToken));

        Assert.Contains("bloqueada", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_throws_unauthorized_when_account_has_no_panel_role()
    {
        var outsider = User with { Roles = ["Instructor"] };

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CreateService(CredentialsCheckResult.Success(outsider), identity: new FakeIdentityService(outsider))
                .LoginAsync(ValidLogin, TestContext.Current.CancellationToken));

        Assert.Equal("Esta conta não tem acesso ao painel.", exception.Message);
        Assert.Empty(_refreshTokens.Tokens);
    }

    [Fact]
    public async Task Refresh_ends_session_when_user_lost_every_panel_role()
    {
        var identity = new FakeIdentityService(User);
        var service = CreateService(CredentialsCheckResult.Success(User), identity: identity);
        var login = await LoginSessionAsync(service);
        identity.UserRoles = [];

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshAsync(login.RefreshToken, TestContext.Current.CancellationToken));

        Assert.False(Assert.Single(_refreshTokens.Tokens).IsActive(_clock.Now.UtcDateTime));
    }

    [Fact]
    public async Task Refresh_rotates_token_in_same_session_when_token_is_active()
    {
        var service = CreateService(CredentialsCheckResult.Success(User));
        var login = await LoginSessionAsync(service);
        _clock.Now = _clock.Now.AddMinutes(20);

        var refreshed = await service.RefreshAsync(login.RefreshToken, TestContext.Current.CancellationToken);

        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
        Assert.Equal(login.SessionExpiresAt, refreshed.SessionExpiresAt);
        Assert.NotNull(_refreshTokens.Tokens[0].UsedAt);
        Assert.Equal(_refreshTokens.Tokens[0].FamilyId, _refreshTokens.Tokens[1].FamilyId);
    }

    [Fact]
    public async Task Refresh_revokes_every_session_when_used_token_is_presented_again()
    {
        var service = CreateService(CredentialsCheckResult.Success(User));
        var login = await LoginSessionAsync(service);
        var otherDevice = await LoginSessionAsync(service);
        await service.RefreshAsync(login.RefreshToken, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshAsync(login.RefreshToken, TestContext.Current.CancellationToken));

        Assert.All(_refreshTokens.Tokens, t => Assert.False(t.IsActive(_clock.Now.UtcDateTime)));
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshAsync(otherDevice.RefreshToken, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Refresh_throws_unauthorized_when_session_expired()
    {
        var service = CreateService(CredentialsCheckResult.Success(User));
        var login = await LoginSessionAsync(service);
        _clock.Now = _clock.Now.AddHours(8).AddSeconds(1);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshAsync(login.RefreshToken, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Refresh_throws_unauthorized_when_token_is_unknown()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CreateService(CredentialsCheckResult.Invalid).RefreshAsync("token-que-nao-existe", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Logout_revokes_only_that_session_when_token_is_known()
    {
        var service = CreateService(CredentialsCheckResult.Success(User));
        var thisDevice = await LoginSessionAsync(service);
        var otherDevice = await LoginSessionAsync(service);
        var rotated = await service.RefreshAsync(thisDevice.RefreshToken, TestContext.Current.CancellationToken);

        await service.LogoutAsync(rotated.RefreshToken, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshAsync(rotated.RefreshToken, TestContext.Current.CancellationToken));
        var stillWorks = await service.RefreshAsync(otherDevice.RefreshToken, TestContext.Current.CancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(stillWorks.AccessToken));
    }

    [Fact]
    public async Task Refresh_does_not_revoke_other_sessions_when_token_was_revoked_by_logout()
    {
        var service = CreateService(CredentialsCheckResult.Success(User));
        var thisDevice = await LoginSessionAsync(service);
        var otherDevice = await LoginSessionAsync(service);
        await service.LogoutAsync(thisDevice.RefreshToken, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshAsync(thisDevice.RefreshToken, TestContext.Current.CancellationToken));

        var stillWorks = await service.RefreshAsync(otherDevice.RefreshToken, TestContext.Current.CancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(stillWorks.AccessToken));
    }

    [Fact]
    public async Task Logout_does_nothing_when_token_is_unknown()
    {
        await CreateService(CredentialsCheckResult.Invalid).LogoutAsync("token-que-nao-existe", TestContext.Current.CancellationToken);

        Assert.Equal(0, _refreshTokens.SaveCount);
    }

    [Fact]
    public async Task GetCurrentUser_throws_not_found_when_user_does_not_exist()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(CredentialsCheckResult.Invalid).GetCurrentUserAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));
    }

    private static string Hash(string token) => FakeTokenService.Hash(token);

    private AuthService CreateService(CredentialsCheckResult result, bool twoFactorRequired = false, FakeIdentityService? identity = null) =>
        new(identity ?? new FakeIdentityService(User, result), _tokens, new FakeTwoFactorPolicy(twoFactorRequired), new FakeEmailSender(), new FakeFrontendLinks(), _refreshTokens, _refreshTokens, _clock);

    private static async Task<AuthSession> LoginSessionAsync(AuthService service) =>
        (await service.LoginAsync(ValidLogin, TestContext.Current.CancellationToken)).Session!;
}
