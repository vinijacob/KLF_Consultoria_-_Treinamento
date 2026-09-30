using Klf.Application.DTOs.Health;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

[Route($"{RoutePrefix}/public/health")]
[Tags("Health")]
public sealed class HealthController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get() => Ok(new HealthResponse("ok"));
}
