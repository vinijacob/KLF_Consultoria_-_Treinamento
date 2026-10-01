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
}
