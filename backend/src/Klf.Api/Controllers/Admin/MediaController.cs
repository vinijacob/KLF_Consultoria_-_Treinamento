using Klf.Api.Authorization;
using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Media;
using Klf.Application.Services.Media;
using Klf.Domain.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Manages the uploaded images (the media library) in the admin panel.</summary>
[Route($"{RoutePrefix}/admin/media")]
[Tags("Admin · Media")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class MediaController(IMediaService mediaService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetMediaById";

    /// <summary>Lists the uploaded images, newest first, with pagination.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponse<MediaAssetResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<MediaAssetResponse>>> ListAsync([FromQuery] PagedRequest request, CancellationToken cancellationToken) =>
        Ok(await mediaService.ListAsync(request, cancellationToken));

    /// <summary>Returns one image.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<MediaAssetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaAssetResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await mediaService.GetByIdAsync(id, cancellationToken));

    /// <summary>
    /// Uploads an image (multipart form with the fields <c>file</c> and, optionally, <c>altText</c>).
    /// JPEG, PNG or WebP up to 5 MB; the type is checked in the file content. Returns the image id to use as cover, logo, photo or album item.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(IMediaService.MaxFileBytes + 1024 * 1024)]
    [ProducesResponseType<MediaAssetResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaAssetResponse>> UploadAsync([FromForm] UploadMediaForm form, CancellationToken cancellationToken)
    {
        if (form.File is null)
        {
            throw new ValidationException("File", "Envie o arquivo da imagem.");
        }

        await using var content = form.File.OpenReadStream();
        var media = await mediaService.UploadAsync(
            new UploadMediaRequest(content, form.File.FileName, form.File.Length, form.AltText),
            cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = media.Id }, media);
    }

    /// <summary>Changes the text alternative of an image.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<MediaAssetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaAssetResponse>> UpdateAsync(Guid id, UpdateMediaRequest request, CancellationToken cancellationToken) =>
        Ok(await mediaService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Deletes an image and its file. Fails with 409 while a cover, logo, photo or album uses it.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await mediaService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}

/// <summary>Form of the image upload.</summary>
public sealed class UploadMediaForm
{
    /// <summary>The image file (JPEG, PNG or WebP, up to 5 MB).</summary>
    public IFormFile? File { get; init; }

    /// <summary>Text alternative for screen readers (up to 200 characters); optional.</summary>
    public string? AltText { get; init; }
}
