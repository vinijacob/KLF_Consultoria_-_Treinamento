using Klf.Application.DTOs.Auth;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Auth;

internal sealed class AuthService(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IAuthService
{
    public async Task<AuthSession> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.CheckCredentialsAsync(request.Email.Trim(), request.Password, cancellationToken);

        return result switch
        {
            { Status: CredentialsCheckStatus.Success, User: { } user } =>
                await IssueSessionAsync(user, Guid.CreateVersion7(), sessionExpiresAt: null, cancellationToken),
            { Status: CredentialsCheckStatus.LockedOut } => throw new UnauthorizedException(
                "Conta bloqueada temporariamente por excesso de tentativas. Tente novamente em alguns minutos."),
            _ => throw new UnauthorizedException("E-mail ou senha inválidos."),
        };
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

        if (user is null)
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

        return new CurrentUserResponse(user.Id, user.Email, user.FullName, user.Roles);
    }

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
