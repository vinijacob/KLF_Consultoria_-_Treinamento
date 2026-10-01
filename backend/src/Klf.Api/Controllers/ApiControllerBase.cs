using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers;

/// <summary>
/// Base class for every API controller. Applies the shared conventions: automatic model binding validation,
/// JSON responses and the documented 500 error.
/// </summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Common route prefix. Use it as <c>[Route($"{RoutePrefix}/public/...")]</c>.</summary>
    public const string RoutePrefix = "api/v1";
}
