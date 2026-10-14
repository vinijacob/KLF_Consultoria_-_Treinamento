namespace Klf.Application.DTOs.Users;

/// <summary>An account of the admin panel.</summary>
/// <param name="Id">Identifier.</param>
/// <param name="Email">E-mail used to sign in.</param>
/// <param name="FullName">Name shown in the panel.</param>
/// <param name="Role"><c>Admin</c> or <c>Editor</c>.</param>
/// <param name="IsActive">Whether the account can sign in.</param>
/// <param name="TwoFactorEnabled">Whether two-factor verification is set up.</param>
/// <param name="InvitePending">Whether the person still has to accept the invitation and choose a password.</param>
/// <param name="CreatedAt">When the account was created (UTC).</param>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    bool TwoFactorEnabled,
    bool InvitePending,
    DateTime CreatedAt);

/// <summary>Invites a person to the panel: creates the account and sends the link to choose a password.</summary>
/// <param name="FullName">Name (2 to 150 characters).</param>
/// <param name="Email">E-mail the person will use to sign in.</param>
/// <param name="Role"><c>Admin</c> or <c>Editor</c>.</param>
public sealed record InviteUserRequest(string FullName, string Email, string Role);

/// <summary>Changes an account.</summary>
/// <param name="FullName">Name (2 to 150 characters).</param>
/// <param name="Role"><c>Admin</c> or <c>Editor</c>. Changing it ends the person's open sessions.</param>
/// <param name="IsActive">Turning it off blocks sign-in and ends every open session.</param>
public sealed record UpdateUserRequest(string FullName, string Role, bool IsActive);

/// <summary>The invitation just created or sent again.</summary>
/// <param name="User">The account.</param>
/// <param name="InviteLink">Link to choose the password. Also sent by e-mail; shown so it can be shared another way (e.g. WhatsApp).</param>
/// <param name="ExpiresAt">When the link stops working (UTC).</param>
public sealed record UserInviteResponse(UserResponse User, Uri InviteLink, DateTime ExpiresAt);

/// <summary>Accepts an invitation by choosing the first password.</summary>
/// <param name="Email">E-mail of the invitation.</param>
/// <param name="Token">Token from the invitation link.</param>
/// <param name="NewPassword">Password (at least 10 characters, with upper and lower case letters, a number and a symbol).</param>
public sealed record AcceptInviteRequest(string Email, string Token, string NewPassword);
