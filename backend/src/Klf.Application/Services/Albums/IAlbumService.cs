using Klf.Application.DTOs.Albums;

namespace Klf.Application.Services.Albums;

/// <summary>Manages the photo albums of the gallery, for the admin panel and the public site.</summary>
public interface IAlbumService
{
    /// <summary>Lists the active albums in display order (public site).</summary>
    Task<IReadOnlyList<AlbumListItemResponse>> ListPublicAsync(CancellationToken cancellationToken);

    /// <summary>Returns an active album with its images (public site).</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">No active album has this slug.</exception>
    Task<PublicAlbumResponse> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Lists every album, active or not, in display order (admin panel).</summary>
    Task<IReadOnlyList<AlbumListItemResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns one album with its images.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The album does not exist.</exception>
    Task<AlbumResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds an empty album.</summary>
    /// <exception cref="Domain.Exceptions.ConflictException">Another album already uses the slug.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">The cover is not an uploaded image.</exception>
    Task<AlbumResponse> CreateAsync(CreateAlbumRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces the fields of an album (not its images).</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The album does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">Another album already uses the slug.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">The cover is not an uploaded image.</exception>
    Task<AlbumResponse> UpdateAsync(Guid id, UpdateAlbumRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces the images of an album with the given list, in order.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The album does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">An image does not exist or appears twice.</exception>
    Task<AlbumResponse> SetItemsAsync(Guid id, SetAlbumItemsRequest request, CancellationToken cancellationToken);

    /// <summary>Soft-deletes an album; the images stay in the library.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The album does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
