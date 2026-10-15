using System.Security.Claims;

using Klf.Application.Interfaces.Audit;
using Klf.Infrastructure.Identity;

namespace Klf.Api.Extensions;

/// <summary>The signed-in user of the current request, read from the access token (nobody for visitors and jobs).</summary>
internal sealed class HttpCurrentActor(IHttpContextAccessor accessor) : ICurrentActor
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User is { Identity.IsAuthenticated: true } user ? user : null;

    public Guid? UserId => Guid.TryParse(User?.FindFirstValue(ClaimTypeNames.Subject), out var id) ? id : null;

    public string? Name => User?.FindFirstValue(ClaimTypeNames.Name);
}
