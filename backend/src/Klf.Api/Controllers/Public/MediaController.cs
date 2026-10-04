using Klf.Application.Services.Media;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Public access to uploaded images by id, for covers, logos and photos.</summary>
[Route($"{RoutePrefix}/public/media")]
[Tags("Media")]
public sealed class MediaController(IMediaService mediaService) : ApiControllerBase
{
    /// <summary>Redirects to the file of an image, so the frontend can use <c>/public/media/{id}</c> as an image address.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Redirect(await mediaService.GetPublicUrlAsync(id, cancellationToken));
}
