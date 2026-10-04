namespace Klf.Application.DTOs.Albums;

/// <summary>Data to add an album. It starts empty: add the images with the items endpoint.</summary>
/// <param name="Title">Name of the album (up to 200 characters).</param>
/// <param name="Slug">Unique URL identifier: lowercase letters, numbers and hyphens (up to 200 characters).</param>
/// <param name="Description">Short text about the album (up to 500 characters); optional.</param>
/// <param name="CoverId">Cover image id (an uploaded image); optional.</param>
/// <param name="DisplayOrder">Position in the public list; lower numbers come first.</param>
/// <param name="IsActive">Whether the album is visible on the public site.</param>
public sealed record CreateAlbumRequest(
    string Title,
    string Slug,
    string? Description,
    Guid? CoverId,
    int DisplayOrder,
    bool IsActive) : IAlbumFields;
