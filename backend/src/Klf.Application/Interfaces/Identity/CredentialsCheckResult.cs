namespace Klf.Application.Interfaces.Identity;

/// <summary>Outcome of checking an e-mail and password.</summary>
public enum CredentialsCheckStatus
{
    /// <summary>The credentials are valid.</summary>
    Success,

    /// <summary>The e-mail does not exist or the password is wrong.</summary>
    InvalidCredentials,

    /// <summary>The account is temporarily locked after too many failed attempts.</summary>
    LockedOut,
}

/// <summary>Result of <see cref="IIdentityService.CheckCredentialsAsync"/>.</summary>
/// <param name="Status">Outcome of the check.</param>
/// <param name="User">The authenticated user; only set when <paramref name="Status"/> is <see cref="CredentialsCheckStatus.Success"/>.</param>
public sealed record CredentialsCheckResult(CredentialsCheckStatus Status, UserAccount? User)
{
    /// <summary>Wrong e-mail or password.</summary>
    public static CredentialsCheckResult Invalid { get; } = new(CredentialsCheckStatus.InvalidCredentials, null);

    /// <summary>Account locked out.</summary>
    public static CredentialsCheckResult LockedOut { get; } = new(CredentialsCheckStatus.LockedOut, null);

    /// <summary>Valid credentials for <paramref name="user"/>.</summary>
    public static CredentialsCheckResult Success(UserAccount user) => new(CredentialsCheckStatus.Success, user);
}
