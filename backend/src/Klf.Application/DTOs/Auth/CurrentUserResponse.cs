namespace Klf.Application.DTOs.Auth;

/// <summary>The signed-in user.</summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">Account e-mail.</param>
/// <param name="FullName">Name shown in the admin panel.</param>
/// <param name="Roles">Roles granted to the user.</param>
/// <param name="TwoFactorEnabled">Whether the user has an authenticator app enrolled.</param>
public sealed record CurrentUserResponse(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles, bool TwoFactorEnabled = false);
