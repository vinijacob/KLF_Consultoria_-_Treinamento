using Klf.Application.DTOs.Albums;
using Klf.Application.Services.Albums;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Photo albums of the public gallery.</summary>
[Route($"{RoutePrefix}/public/albums")]
[Tags("Albums")]
public sealed class AlbumsController(IAlbumService albumService) : ApiControllerBase
{
    /// <summary>Lists the active albums in display order.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AlbumListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlbumListItemResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await albumService.ListPublicAsync(cancellationToken));

    /// <summary>Returns an active album with its images.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType<PublicAlbumResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicAlbumResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        Ok(await albumService.GetPublicBySlugAsync(slug, cancellationToken));
}
