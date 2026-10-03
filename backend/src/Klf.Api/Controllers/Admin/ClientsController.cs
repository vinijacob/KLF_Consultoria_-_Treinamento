using Klf.Api.Authorization;
using Klf.Application.DTOs.Clients;
using Klf.Application.Services.Clients;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Manages the companies and stores trained by KLF in the admin panel.</summary>
[Route($"{RoutePrefix}/admin/clients")]
[Tags("Admin · Clients")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class ClientsController(IClientService clientService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetClientById";

    /// <summary>Lists every client, active or not, in display order.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClientResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await clientService.ListAsync(cancellationToken));

    /// <summary>Returns one client.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await clientService.GetByIdAsync(id, cancellationToken));

    /// <summary>Adds a client.</summary>
    [HttpPost]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientResponse>> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken)
    {
        var client = await clientService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = client.Id }, client);
    }

    /// <summary>Replaces every editable field of a client.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ClientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken) =>
        Ok(await clientService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Removes a client from the site (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await clientService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
