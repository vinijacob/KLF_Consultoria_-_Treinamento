using Klf.Application.DTOs.Career;
using Klf.Application.Services.Career;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Kilciene's career timeline shown on the public site.</summary>
[Route($"{RoutePrefix}/public/career")]
[Tags("Career")]
public sealed class CareerController(ICareerEntryService careerEntryService) : ApiControllerBase
{
    /// <summary>Lists the career timeline (education, certifications and experience), ordered for display.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CareerEntryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CareerEntryResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await careerEntryService.ListAsync(cancellationToken));
}
