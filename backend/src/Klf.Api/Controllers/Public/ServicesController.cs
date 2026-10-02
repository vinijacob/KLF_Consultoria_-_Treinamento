using Klf.Application.DTOs.Catalog;
using Klf.Application.Services.Catalog;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Services offered by KLF, shown on the public site.</summary>
[Route($"{RoutePrefix}/public/services")]
[Tags("Services")]
public sealed class ServicesController(IServiceCatalogService catalogService) : ApiControllerBase
{
    /// <summary>Lists the active services in display order.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServiceListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceListItemResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await catalogService.ListPublicAsync(cancellationToken));

    /// <summary>Returns an active service by its slug, for its detail page.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType<PublicServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicServiceResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        Ok(await catalogService.GetPublicBySlugAsync(slug, cancellationToken));
}
