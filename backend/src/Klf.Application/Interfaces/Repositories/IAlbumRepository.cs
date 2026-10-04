using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="Album"/>. Soft-deleted albums are never returned; items are always loaded.</summary>
public interface IAlbumRepository
{
    /// <summary>Returns the albums with their items, ordered by <see cref="Album.DisplayOrder"/> (then title), read-only.</summary>
    /// <param name="onlyActive">When <see langword="true"/>, skips inactive albums (public site).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<Album>> ListAsync(bool onlyActive, CancellationToken cancellationToken);

    /// <summary>Returns the album with its items, tracked for changes, or <see langword="null"/>.</summary>
    Task<Album?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns the active album with its items, read-only, or <see langword="null"/>.</summary>
    Task<Album?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Whether another album already uses the slug; <paramref name="excludingId"/> skips the album being edited.</summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken);

    /// <summary>Marks a new album to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Album album);

    /// <summary>Marks an album to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(Album album);
}
