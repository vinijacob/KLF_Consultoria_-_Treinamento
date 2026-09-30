using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase : ControllerBase
{
    public const string RoutePrefix = "api/v1";
}
