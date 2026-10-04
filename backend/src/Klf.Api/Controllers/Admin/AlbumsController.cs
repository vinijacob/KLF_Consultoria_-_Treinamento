using Klf.Api.Authorization;
using Klf.Application.DTOs.Albums;
using Klf.Application.Services.Albums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Manages the photo albums of the gallery in the admin panel.</summary>
[Route($"{RoutePrefix}/admin/albums")]
[Tags("Admin · Albums")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class AlbumsController(IAlbumService albumService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetAlbumById";

    /// <summary>Lists every album, active or not, in display order.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AlbumListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlbumListItemResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await albumService.ListAsync(cancellationToken));

    /// <summary>Returns one album with its images.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<AlbumResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlbumResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await albumService.GetByIdAsync(id, cancellationToken));

    /// <summary>Adds an empty album. Add its images with <c>PUT {id}/items</c>.</summary>
    [HttpPost]
    [ProducesResponseType<AlbumResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AlbumResponse>> CreateAsync(CreateAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = await albumService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = album.Id }, album);
    }

    /// <summary>Replaces the fields of an album (not its images).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<AlbumResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AlbumResponse>> UpdateAsync(Guid id, UpdateAlbumRequest request, CancellationToken cancellationToken) =>
        Ok(await albumService.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Replaces the images of an album with the given list, in order. Images left out of the list are removed from the album
    /// (they stay in the media library).
    /// </summary>
    [HttpPut("{id:guid}/items")]
    [ProducesResponseType<AlbumResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlbumResponse>> SetItemsAsync(Guid id, SetAlbumItemsRequest request, CancellationToken cancellationToken) =>
        Ok(await albumService.SetItemsAsync(id, request, cancellationToken));

    /// <summary>Removes an album from the site (soft delete). Its images stay in the media library.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await albumService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
