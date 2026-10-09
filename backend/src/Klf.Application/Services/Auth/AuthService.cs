using Klf.Application.DTOs.Auth;
using Klf.Application.Interfaces.Email;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Links;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Auth;

internal sealed class AuthService(
    IIdentityService identityService,
    ITokenService tokenService,
    ITwoFactorPolicy twoFactorPolicy,
    IEmailSender emailSender,
    IFrontendLinks links,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IAuthService
{
    private const int PasswordResetLifetimeMinutes = 30;

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.CheckCredentialsAsync(request.Email.Trim(), request.Password, cancellationToken);

        if (result.User is { } account && !Roles.CanSignIn(account.Roles))
        {
            throw new UnauthorizedException("Esta conta não tem acesso ao painel.");
        }

        return result switch
        {
            { Status: CredentialsCheckStatus.Success, User: { } user } when user.TwoFactorEnabled || twoFactorPolicy.IsRequired =>
                new LoginResult(null, new TwoFactorChallenge(tokenService.CreateTwoFactorChallenge(user.Id), SetupRequired: !user.TwoFactorEnabled)),
            { Status: CredentialsCheckStatus.Success, User: { } user } =>
                new LoginResult(await StartNewSessionAsync(user, cancellationToken), null),
            { Status: CredentialsCheckStatus.LockedOut } => throw new UnauthorizedException(
                "Conta bloqueada temporariamente por excesso de tentativas. Tente novamente em alguns minutos."),
            _ => throw new UnauthorizedException("E-mail ou senha inválidos."),
        };
    }

    public async Task<AuthSession> VerifyTwoFactorAsync(TwoFactorCodeRequest request, CancellationToken cancellationToken)
    {
        var user = await ReadChallengeUserAsync(request.TwoFactorToken, cancellationToken);

        if (!user.TwoFactorEnabled)
        {
            throw ChallengeExpired();
        }

        var status = await identityService.VerifyTwoFactorCodeAsync(user.Id, request.Code, cancellationToken);

        return await StartSessionOrFailAsync(status, user, cancellationToken);
    }

    public async Task<AuthSession> RecoverWithCodeAsync(TwoFactorRecoveryRequest request, CancellationToken cancellationToken)
    {
        var user = await ReadChallengeUserAsync(request.TwoFactorToken, cancellationToken);

        if (!user.TwoFactorEnabled)
        {
            throw ChallengeExpired();
        }

        var status = await identityService.RedeemRecoveryCodeAsync(user.Id, request.RecoveryCode, cancellationToken);

        return await StartSessionOrFailAsync(status, user, cancellationToken);
    }

    public async Task<TwoFactorSetup> StartTwoFactorSetupAsync(TwoFactorSetupRequest request, CancellationToken cancellationToken)
    {
        var user = await ReadChallengeUserAsync(request.TwoFactorToken, cancellationToken);
        EnsureNotEnabled(user);

        return await identityService.StartTwoFactorSetupAsync(user.Id, cancellationToken)
            ?? throw ChallengeExpired();
    }

    public async Task<TwoFactorEnabledResult> EnableTwoFactorAsync(TwoFactorCodeRequest request, CancellationToken cancellationToken)
    {
        var user = await ReadChallengeUserAsync(request.TwoFactorToken, cancellationToken);
        EnsureNotEnabled(user);

        var result = await identityService.EnableTwoFactorAsync(user.Id, request.Code, cancellationToken);
        ThrowIfNotSuccess(result.Status);

        var session = await StartNewSessionAsync(user with { TwoFactorEnabled = true }, cancellationToken);

        return new TwoFactorEnabledResult(session, result.RecoveryCodes);
    }

    public async Task<IReadOnlyList<string>> RegenerateRecoveryCodesAsync(
        Guid userId,
        RegenerateRecoveryCodesRequest request,
        CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(userId, cancellationToken)
            ?? throw NotFoundException.For("Usuário", userId);

        if (!user.TwoFactorEnabled)
        {
            throw new ConflictException("Ative a verificação em duas etapas antes de gerar códigos de recuperação.");
        }

        var result = await identityService.RegenerateRecoveryCodesAsync(userId, request.Code, cancellationToken);
        ThrowIfNotSuccess(result.Status);

        return result.RecoveryCodes;
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var reset = await identityService.CreatePasswordResetTokenAsync(request.Email.Trim(), cancellationToken);

        if (reset is null)
        {
            return;
        }

        var link = links.PasswordReset(reset.Email, reset.Token);

        await emailSender.SendAsync(
            PasswordEmailTemplates.ResetLink(reset.Email, reset.FullName, link, PasswordResetLifetimeMinutes),
            cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.ResetPasswordAsync(request.Email.Trim(), request.Token, request.NewPassword, cancellationToken);

        switch (result)
        {
            case { Status: PasswordResetStatus.Success, User: { } user }:
                await RevokeAllSessionsAsync(user.Id, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
                await emailSender.SendAsync(PasswordEmailTemplates.PasswordChanged(user.Email, user.FullName), cancellationToken);
                return;
            case { Status: PasswordResetStatus.WeakPassword }:
                throw new ValidationException(new Dictionary<string, string[]> { ["NewPassword"] = [.. result.Errors] });
            default:
                throw new ValidationException("Token", "Link inválido ou expirado. Solicite um novo.");
        }
    }

    public async Task<AuthSession> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var stored = await refreshTokens.GetByHashAsync(tokenService.HashRefreshToken(refreshToken), cancellationToken)
            ?? throw SessionEnded();

        if (stored.UsedAt is not null)
        {
            await RevokeAllSessionsAsync(stored.UserId, now, cancellationToken);
            throw SessionEnded();
        }

        if (!stored.IsActive(now))
        {
            throw SessionEnded();
        }

        var user = await identityService.FindByIdAsync(stored.UserId, cancellationToken);

        if (user is null || !Roles.CanSignIn(user.Roles))
        {
            stored.Revoke(now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw SessionEnded();
        }

        stored.MarkAsUsed(now);

        return await IssueSessionAsync(user, stored.FamilyId, stored.ExpiresAt, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var stored = await refreshTokens.GetByHashAsync(tokenService.HashRefreshToken(refreshToken), cancellationToken);

        if (stored is null)
        {
            return;
        }

        foreach (var token in await refreshTokens.ListActiveByFamilyAsync(stored.FamilyId, now, cancellationToken))
        {
            token.Revoke(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(userId, cancellationToken)
            ?? throw NotFoundException.For("Usuário", userId);

        return new CurrentUserResponse(user.Id, user.Email, user.FullName, user.Roles, user.TwoFactorEnabled);
    }

    private Task<AuthSession> StartNewSessionAsync(UserAccount user, CancellationToken cancellationToken) =>
        IssueSessionAsync(user, Guid.CreateVersion7(), sessionExpiresAt: null, cancellationToken);

    private async Task<AuthSession> StartSessionOrFailAsync(TwoFactorCheckStatus status, UserAccount user, CancellationToken cancellationToken)
    {
        ThrowIfNotSuccess(status);

        return await StartNewSessionAsync(user, cancellationToken);
    }

    private static void ThrowIfNotSuccess(TwoFactorCheckStatus status)
    {
        switch (status)
        {
            case TwoFactorCheckStatus.Success:
                return;
            case TwoFactorCheckStatus.LockedOut:
                throw new UnauthorizedException("Conta bloqueada temporariamente por excesso de tentativas. Tente novamente em alguns minutos.");
            default:
                throw new UnauthorizedException("Código inválido.");
        }
    }

    private static void EnsureNotEnabled(UserAccount user)
    {
        if (user.TwoFactorEnabled)
        {
            throw new ConflictException("A verificação em duas etapas já está ativada.");
        }
    }

    private async Task<UserAccount> ReadChallengeUserAsync(string token, CancellationToken cancellationToken)
    {
        var userId = await tokenService.ReadTwoFactorChallengeAsync(token) ?? throw ChallengeExpired();
        var user = await identityService.FindByIdAsync(userId, cancellationToken);

        return user is not null && Roles.CanSignIn(user.Roles) ? user : throw ChallengeExpired();
    }

    private static UnauthorizedException ChallengeExpired() =>
        new("A verificação expirou. Entre novamente com e-mail e senha.");

    private async Task<AuthSession> IssueSessionAsync(
        UserAccount user,
        Guid familyId,
        DateTime? sessionExpiresAt,
        CancellationToken cancellationToken)
    {
        var accessToken = tokenService.Generate(user);
        var refreshToken = tokenService.CreateRefreshToken();
        var expiresAt = sessionExpiresAt ?? refreshToken.ExpiresAt;

        refreshTokens.Add(new RefreshToken(user.Id, familyId, refreshToken.Hash, expiresAt));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthSession(accessToken.Token, accessToken.ExpiresAt, refreshToken.Token, expiresAt);
    }

    private async Task RevokeAllSessionsAsync(Guid userId, DateTime now, CancellationToken cancellationToken)
    {
        foreach (var token in await refreshTokens.ListActiveByUserAsync(userId, now, cancellationToken))
        {
            token.Revoke(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static UnauthorizedException SessionEnded() =>
        new("Sua sessão expirou. Entre novamente.");
}
