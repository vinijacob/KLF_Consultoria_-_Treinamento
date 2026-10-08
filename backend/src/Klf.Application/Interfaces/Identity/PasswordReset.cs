namespace Klf.Application.Interfaces.Identity;

/// <summary>A freshly issued password reset token and who it belongs to.</summary>
/// <param name="Email">Account e-mail.</param>
/// <param name="FullName">Name of the user, for the greeting.</param>
/// <param name="Token">Single-use token, valid for a short time. Goes only into the e-mail link.</param>
public sealed record PasswordResetToken(string Email, string FullName, string Token);

/// <summary>Outcome of a password reset.</summary>
public enum PasswordResetStatus
{
    /// <summary>The password was changed.</summary>
    Success,

    /// <summary>The token is wrong, expired, already used, or the e-mail does not exist.</summary>
    InvalidToken,

    /// <summary>The new password does not meet the password policy.</summary>
    WeakPassword,
}

/// <summary>Result of <see cref="IIdentityService.ResetPasswordAsync"/>.</summary>
/// <param name="Status">Outcome.</param>
/// <param name="Errors">Password policy messages in Portuguese; only when <paramref name="Status"/> is <see cref="PasswordResetStatus.WeakPassword"/>.</param>
/// <param name="User">The user whose password changed; only when <paramref name="Status"/> is <see cref="PasswordResetStatus.Success"/>.</param>
public sealed record PasswordResetResult(PasswordResetStatus Status, IReadOnlyList<string> Errors, UserAccount? User);
