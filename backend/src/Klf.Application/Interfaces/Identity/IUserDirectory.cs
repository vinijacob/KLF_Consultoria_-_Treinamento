namespace Klf.Application.Interfaces.Identity;

/// <summary>An account of the admin panel as the team management screen sees it.</summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">Account e-mail.</param>
/// <param name="FullName">Name shown in the panel.</param>
/// <param name="Role">Panel role (<c>Admin</c> or <c>Editor</c>); empty if the account has none.</param>
/// <param name="IsActive">Whether the account is enabled.</param>
/// <param name="TwoFactorEnabled">Whether the person finished enrolling an authenticator app.</param>
/// <param name="InvitePending">Whether the person has not chosen a password yet (invitation not accepted).</param>
/// <param name="CreatedAt">When the account was created, in UTC.</param>
public sealed record ManagedUser(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    bool TwoFactorEnabled,
    bool InvitePending,
    DateTime CreatedAt);

/// <summary>
/// Creates and changes the accounts of the admin panel (RF35). Implemented in Infrastructure on top of ASP.NET Core
/// Identity. The rules (who may change what) live in the Application service, not here.
/// </summary>
public interface IUserDirectory
{
    /// <summary>Every account, ordered by name.</summary>
    Task<IReadOnlyList<ManagedUser>> ListAsync(CancellationToken cancellationToken);

    /// <summary>The account with the given id, or <see langword="null"/>.</summary>
    Task<ManagedUser?> FindAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Whether any account already uses this e-mail (case ignored).</summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    /// <summary>Creates an active account without a password, with a single role. The person sets the password through the invitation.</summary>
    Task<ManagedUser> CreateAsync(string email, string fullName, string role, CancellationToken cancellationToken);

    /// <summary>Replaces name, role and active flag. Deactivating also invalidates pending password and invitation links.</summary>
    Task<ManagedUser> UpdateAsync(Guid userId, string fullName, string role, bool isActive, CancellationToken cancellationToken);

    /// <summary>Issues a single-use invitation token, valid for <see cref="InviteLifetimeDays"/> days.</summary>
    Task<string> CreateInviteTokenAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the first password when the invitation token is valid and the account is active and still without a password.
    /// The token stops working afterwards.
    /// </summary>
    Task<PasswordResetResult> AcceptInviteAsync(string email, string token, string password, CancellationToken cancellationToken);

    /// <summary>Turns two-factor off and discards the authenticator key and recovery codes: the next sign-in asks to enroll again.</summary>
    Task ResetTwoFactorAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>How long an invitation link works.</summary>
    const int InviteLifetimeDays = 7;
}
