using Klf.Application.DTOs.Clients;
using Klf.Application.Services.Clients;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Companies and stores trained by KLF, shown on the public site.</summary>
[Route($"{RoutePrefix}/public/clients")]
[Tags("Clients")]
public sealed class ClientsController(IClientService clientService) : ApiControllerBase
{
    /// <summary>Lists the active clients in display order.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PublicClientResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PublicClientResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await clientService.ListPublicAsync(cancellationToken));
}
