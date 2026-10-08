namespace Klf.Application.Interfaces.Identity;

/// <summary>Access to user accounts. Implemented in Infrastructure on top of ASP.NET Core Identity.</summary>
public interface IIdentityService
{
    /// <summary>
    /// Checks the credentials and applies the lockout policy: each wrong password counts as a failed attempt,
    /// and a successful sign-in resets the counter.
    /// </summary>
    Task<CredentialsCheckResult> CheckCredentialsAsync(string email, string password, CancellationToken cancellationToken);

    /// <summary>Finds a user by identifier, or returns <see langword="null"/> if it does not exist.</summary>
    Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new authenticator key for a user who has not finished the enrollment (replacing any unfinished one).
    /// Returns <see langword="null"/> if the user does not exist.
    /// </summary>
    Task<TwoFactorSetup?> StartTwoFactorSetupAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Checks a 6-digit authenticator code. A wrong code counts as a failed attempt for the lockout policy.</summary>
    Task<TwoFactorCheckStatus> VerifyTwoFactorCodeAsync(Guid userId, string code, CancellationToken cancellationToken);

    /// <summary>Checks and consumes a one-time recovery code. A wrong code counts as a failed attempt for the lockout policy.</summary>
    Task<TwoFactorCheckStatus> RedeemRecoveryCodeAsync(Guid userId, string recoveryCode, CancellationToken cancellationToken);

    /// <summary>
    /// Finishes the enrollment: when the code is valid, turns two-factor on and returns 10 new recovery codes.
    /// </summary>
    Task<TwoFactorEnableResult> EnableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken);

    /// <summary>When the code is valid, replaces all recovery codes with 10 new ones (the old ones stop working).</summary>
    Task<TwoFactorEnableResult> RegenerateRecoveryCodesAsync(Guid userId, string code, CancellationToken cancellationToken);

    /// <summary>
    /// Issues a password reset token (valid for 30 minutes, single use) for the e-mail.
    /// Returns <see langword="null"/> if no user has this e-mail.
    /// </summary>
    Task<PasswordResetToken?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Changes the password when the token is valid. A successful reset also clears the lockout.
    /// The token stops working afterwards, so it cannot be used twice.
    /// </summary>
    Task<PasswordResetResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);
}
