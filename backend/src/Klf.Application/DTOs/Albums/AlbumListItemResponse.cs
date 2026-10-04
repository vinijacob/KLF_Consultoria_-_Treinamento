namespace Klf.Application.DTOs.Albums;

/// <summary>An album in a list: cover and count only, no images.</summary>
/// <param name="Id">Album identifier.</param>
/// <param name="Title">Name of the album.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="Description">Short text about the album.</param>
/// <param name="CoverUrl">Public address of the cover.</param>
/// <param name="ItemCount">Number of images.</param>
/// <param name="DisplayOrder">Position in the list.</param>
/// <param name="IsActive">Whether the album is visible on the public site.</param>
public sealed record AlbumListItemResponse(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string? CoverUrl,
    int ItemCount,
    int DisplayOrder,
    bool IsActive);
