namespace Klf.Application.Interfaces.Identity;

/// <summary>Identity user data the Application layer needs, without depending on ASP.NET Core Identity.</summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">Account e-mail.</param>
/// <param name="FullName">Name shown in the admin panel.</param>
/// <param name="Roles">Roles granted to the user.</param>
public sealed record UserAccount(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles);
