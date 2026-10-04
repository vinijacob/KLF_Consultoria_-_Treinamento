namespace Klf.Application.DTOs.Albums;

/// <summary>An album with its images, for the admin panel.</summary>
/// <param name="Id">Album identifier.</param>
/// <param name="Title">Name of the album.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="Description">Short text about the album.</param>
/// <param name="CoverId">Cover image id.</param>
/// <param name="CoverUrl">Public address of the cover.</param>
/// <param name="DisplayOrder">Position in the public list.</param>
/// <param name="IsActive">Whether the album is visible on the public site.</param>
/// <param name="Items">Images in display order.</param>
public sealed record AlbumResponse(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    Guid? CoverId,
    string? CoverUrl,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<AlbumItemResponse> Items);

/// <summary>An image inside an album.</summary>
/// <param name="MediaAssetId">Image identifier.</param>
/// <param name="Url">Public address of the file.</param>
/// <param name="AltText">Text alternative for screen readers.</param>
/// <param name="Caption">Caption shown under the image.</param>
/// <param name="DisplayOrder">Position in the album.</param>
public sealed record AlbumItemResponse(Guid MediaAssetId, string Url, string? AltText, string? Caption, int DisplayOrder);
