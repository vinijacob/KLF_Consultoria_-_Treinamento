using Klf.Api.Extensions;
using Klf.Application.DTOs.Users;
using Klf.Application.Services.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Klf.Api.Controllers.Auth;

/// <summary>Invitations to the admin panel, accepted by the invited person (no session yet).</summary>
[Route($"{RoutePrefix}/auth/invite")]
[Tags("Auth")]
[AllowAnonymous]
public sealed class InvitesController(IUserManagementService userService) : ApiControllerBase
{
    /// <summary>Accepts an invitation by choosing the first password.</summary>
    /// <remarks>
    /// The link is valid for 7 days and works once. Afterwards the person signs in normally and, on the first access,
    /// enrolls the authenticator app.
    /// </remarks>
    [HttpPost("accept")]
    [EnableRateLimiting(AuthenticationExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AcceptAsync(AcceptInviteRequest request, CancellationToken cancellationToken)
    {
        await userService.AcceptInviteAsync(request, cancellationToken);

        return NoContent();
    }
}
