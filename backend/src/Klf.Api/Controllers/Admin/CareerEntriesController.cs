using Klf.Api.Authorization;
using Klf.Application.DTOs.Career;
using Klf.Application.Services.Career;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Manages the career timeline in the admin panel.</summary>
[Route($"{RoutePrefix}/admin/career")]
[Tags("Admin · Career")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class CareerEntriesController(ICareerEntryService careerEntryService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetCareerEntryById";

    /// <summary>Lists every career entry.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CareerEntryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CareerEntryResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await careerEntryService.ListAsync(cancellationToken));

    /// <summary>Returns one career entry.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<CareerEntryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CareerEntryResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await careerEntryService.GetByIdAsync(id, cancellationToken));

    /// <summary>Adds a career entry.</summary>
    [HttpPost]
    [ProducesResponseType<CareerEntryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CareerEntryResponse>> CreateAsync(CreateCareerEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await careerEntryService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = entry.Id }, entry);
    }

    /// <summary>Replaces every field of a career entry.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<CareerEntryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CareerEntryResponse>> UpdateAsync(Guid id, UpdateCareerEntryRequest request, CancellationToken cancellationToken) =>
        Ok(await careerEntryService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Removes a career entry from the site (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await careerEntryService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
