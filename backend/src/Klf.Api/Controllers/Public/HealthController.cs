using Klf.Application.DTOs.Health;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Health check used by the hosting platform and monitoring.</summary>
[Route($"{RoutePrefix}/public/health")]
[Tags("Health")]
public sealed class HealthController : ApiControllerBase
{
    /// <summary>Returns <c>ok</c> when the API is up.</summary>
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get() => Ok(new HealthResponse("ok"));
}
