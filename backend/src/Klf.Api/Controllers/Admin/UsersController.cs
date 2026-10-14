using Klf.Api.Authorization;
using Klf.Api.Extensions;
using Klf.Application.DTOs.Users;
using Klf.Application.Services.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Team of the admin panel (RF35): invite people, change role, deactivate and reset two-factor. Admin only.</summary>
[Route($"{RoutePrefix}/admin/users")]
[Tags("Admin · Users")]
[Authorize(Policy = Policies.ManageUsers)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class UsersController(IUserManagementService userService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetUserById";

    /// <summary>Lists every account, ordered by name.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await userService.ListAsync(cancellationToken));

    /// <summary>Returns one account.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await userService.GetAsync(id, cancellationToken));

    /// <summary>Invites a person: creates the account (without password) and sends the link to choose one.</summary>
    /// <remarks>The link also comes in the response, so it can be shared by WhatsApp while e-mail is not set up.</remarks>
    [HttpPost]
    [ProducesResponseType<UserInviteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserInviteResponse>> InviteAsync(InviteUserRequest request, CancellationToken cancellationToken)
    {
        var invite = await userService.InviteAsync(User.GetUserId(), request, cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = invite.User.Id }, invite);
    }

    /// <summary>Changes name, role and active flag. Deactivating or changing the role ends the person's sessions.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        Ok(await userService.UpdateAsync(User.GetUserId(), id, request, cancellationToken));

    /// <summary>Sends a new invitation link to someone who has not chosen a password yet.</summary>
    [HttpPost("{id:guid}/invite")]
    [ProducesResponseType<UserInviteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserInviteResponse>> ResendInviteAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await userService.ResendInviteAsync(User.GetUserId(), id, cancellationToken));

    /// <summary>Resets someone's two-factor (lost phone): ends their sessions; the next sign-in asks to enroll again.</summary>
    [HttpPost("{id:guid}/reset-two-factor")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> ResetTwoFactorAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await userService.ResetTwoFactorAsync(User.GetUserId(), id, cancellationToken));
}
