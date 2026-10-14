namespace Klf.Application.Interfaces.Identity;

/// <summary>Identity user data the Application layer needs, without depending on ASP.NET Core Identity.</summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">Account e-mail.</param>
/// <param name="FullName">Name shown in the admin panel.</param>
/// <param name="Roles">Roles granted to the user.</param>
/// <param name="TwoFactorEnabled">Whether the user finished enrolling an authenticator app.</param>
/// <param name="IsActive">Whether the account is enabled. Deactivated accounts cannot sign in or renew a session.</param>
public sealed record UserAccount(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles, bool TwoFactorEnabled = false, bool IsActive = true)
{
    /// <summary>Whether the account may use the admin panel: active and with a panel role.</summary>
    public bool CanSignIn() => IsActive && Klf.Domain.Common.Roles.CanSignIn(Roles);
}
