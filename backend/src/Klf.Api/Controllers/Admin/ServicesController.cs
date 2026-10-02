using Klf.Api.Authorization;
using Klf.Application.DTOs.Catalog;
using Klf.Application.Services.Catalog;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Manages the services offered by KLF in the admin panel.</summary>
[Route($"{RoutePrefix}/admin/services")]
[Tags("Admin · Services")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class ServicesController(IServiceCatalogService catalogService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetServiceById";

    /// <summary>Lists every service, active or not, in display order.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServiceListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceListItemResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await catalogService.ListAsync(cancellationToken));

    /// <summary>Returns one service with every field.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<ServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await catalogService.GetByIdAsync(id, cancellationToken));

    /// <summary>Adds a service to the catalog.</summary>
    [HttpPost]
    [ProducesResponseType<ServiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResponse>> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await catalogService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = service.Id }, service);
    }

    /// <summary>Replaces every editable field of a service.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResponse>> UpdateAsync(Guid id, UpdateServiceRequest request, CancellationToken cancellationToken) =>
        Ok(await catalogService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Removes a service from the site (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await catalogService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
