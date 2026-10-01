using Klf.Application.DTOs.Auth;

namespace Klf.Application.Services.Auth;

/// <summary>Sign-in, session refresh and current-user operations for the admin panel.</summary>
public interface IAuthService
{
    /// <summary>Validates the credentials and starts a new session.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Wrong credentials or locked account.</exception>
    Task<AuthSession> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Exchanges a refresh token for a new access token and a new refresh token of the same session.
    /// Presenting an already exchanged token (a sign of theft) revokes every session of the user;
    /// a token revoked by logout is just rejected.
    /// </summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Unknown, reused or expired token.</exception>
    Task<AuthSession> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Ends the session the refresh token belongs to. Never fails: an unknown or already ended token is ignored,
    /// so signing out twice (or with an expired session) is harmless.
    /// </summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Returns the data of the signed-in user.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The user no longer exists.</exception>
    Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
