namespace Klf.Application.DTOs.Albums;

/// <summary>An active album as its public page needs it.</summary>
/// <param name="Id">Album identifier.</param>
/// <param name="Title">Name of the album.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="Description">Short text about the album.</param>
/// <param name="CoverUrl">Public address of the cover.</param>
/// <param name="Items">Images in display order.</param>
public sealed record PublicAlbumResponse(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string? CoverUrl,
    IReadOnlyList<PublicAlbumItemResponse> Items);

/// <summary>An image of a public album.</summary>
/// <param name="Url">Public address of the file.</param>
/// <param name="AltText">Text alternative for screen readers.</param>
/// <param name="Caption">Caption shown under the image.</param>
public sealed record PublicAlbumItemResponse(string Url, string? AltText, string? Caption);
