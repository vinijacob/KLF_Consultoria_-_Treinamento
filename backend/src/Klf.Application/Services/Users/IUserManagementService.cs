using Klf.Application.DTOs.Users;

namespace Klf.Application.Services.Users;

/// <summary>Team management of the admin panel (RF35): invite, change, deactivate and reset two-factor. Admin only.</summary>
public interface IUserManagementService
{
    /// <summary>Every account, ordered by name.</summary>
    Task<IReadOnlyList<UserResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>One account.</summary>
    Task<UserResponse> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Creates the account and sends the invitation (e-mail + link returned to the caller).</summary>
    Task<UserInviteResponse> InviteAsync(Guid actorId, InviteUserRequest request, CancellationToken cancellationToken);

    /// <summary>Issues a new invitation link for someone who has not chosen a password yet.</summary>
    Task<UserInviteResponse> ResendInviteAsync(Guid actorId, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Changes name, role and active flag. Nobody can deactivate themselves or drop their own Admin role, and there is always
    /// at least one active Admin. Deactivating or changing the role ends the person's sessions.
    /// </summary>
    Task<UserResponse> UpdateAsync(Guid actorId, Guid userId, UpdateUserRequest request, CancellationToken cancellationToken);

    /// <summary>Resets someone else's two-factor (lost phone): ends their sessions and tells them by e-mail.</summary>
    Task<UserResponse> ResetTwoFactorAsync(Guid actorId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Accepts an invitation by choosing the first password (public route, no session).</summary>
    Task AcceptInviteAsync(AcceptInviteRequest request, CancellationToken cancellationToken);
}
