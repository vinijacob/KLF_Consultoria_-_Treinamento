using Klf.Application.DTOs.Auth;
using Klf.Application.Interfaces.Identity;

namespace Klf.Application.Services.Auth;

/// <summary>Sign-in, session refresh and current-user operations for the admin panel.</summary>
public interface IAuthService
{
    /// <summary>
    /// Validates the credentials. When two-factor applies (the user has it enabled, or it is required and the user must enroll),
    /// no session is started: a challenge for the second step is returned instead.
    /// </summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Wrong credentials or locked account.</exception>
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    /// <summary>Second step of the sign-in: checks the authenticator code and starts the session.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Expired token, wrong code or locked account.</exception>
    Task<AuthSession> VerifyTwoFactorAsync(TwoFactorCodeRequest request, CancellationToken cancellationToken);

    /// <summary>Second step of the sign-in with a one-time recovery code: consumes the code and starts the session.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Expired token, wrong code or locked account.</exception>
    Task<AuthSession> RecoverWithCodeAsync(TwoFactorRecoveryRequest request, CancellationToken cancellationToken);

    /// <summary>Starts the authenticator enrollment: returns the key and the <c>otpauth://</c> address for the QR Code.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Expired token.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">Two-factor is already enabled.</exception>
    Task<TwoFactorSetup> StartTwoFactorSetupAsync(TwoFactorSetupRequest request, CancellationToken cancellationToken);

    /// <summary>Finishes the enrollment with a valid code: enables two-factor, starts the session and returns the recovery codes.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Expired token, wrong code or locked account.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">Two-factor is already enabled.</exception>
    Task<TwoFactorEnabledResult> EnableTwoFactorAsync(TwoFactorCodeRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces the recovery codes of a signed-in user after checking the current authenticator code.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Wrong code or locked account.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">Two-factor is not enabled yet.</exception>
    Task<IReadOnlyList<string>> RegenerateRecoveryCodesAsync(Guid userId, RegenerateRecoveryCodesRequest request, CancellationToken cancellationToken);

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

    /// <summary>
    /// Sends a password reset link by e-mail when an account with this e-mail exists. Always completes the same way
    /// whether the account exists or not, so the endpoint cannot be used to discover which e-mails are registered.
    /// </summary>
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Changes the password with the token from the e-mail, ends every session of the user and notifies the user by e-mail.
    /// Two-factor still applies on the next sign-in.
    /// </summary>
    /// <exception cref="Domain.Exceptions.ValidationException">Invalid or expired link, or a password that breaks the policy.</exception>
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
}
