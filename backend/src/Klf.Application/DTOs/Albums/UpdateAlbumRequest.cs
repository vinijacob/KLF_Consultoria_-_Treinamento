namespace Klf.Application.DTOs.Albums;

/// <summary>Data to replace the fields of an album. The images are changed with the items endpoint.</summary>
/// <param name="Title">Name of the album (up to 200 characters).</param>
/// <param name="Slug">Unique URL identifier: lowercase letters, numbers and hyphens (up to 200 characters).</param>
/// <param name="Description">Short text about the album (up to 500 characters); optional.</param>
/// <param name="CoverId">Cover image id (an uploaded image); optional.</param>
/// <param name="DisplayOrder">Position in the list (0 = first); leave empty to keep the current one. If another item holds it, the two swap places.</param>
/// <param name="IsActive">Whether the album is visible on the public site.</param>
public sealed record UpdateAlbumRequest(
    string Title,
    string Slug,
    string? Description,
    Guid? CoverId,
    int? DisplayOrder,
    bool IsActive) : IAlbumFields;
