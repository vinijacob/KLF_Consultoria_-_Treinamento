using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="MediaAsset"/>. Soft-deleted images are never returned.</summary>
public interface IMediaAssetRepository
{
    /// <summary>Returns a page of images, newest first, read-only, plus the total count.</summary>
    /// <param name="skip">Number of images to skip.</param>
    /// <param name="take">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<(IReadOnlyList<MediaAsset> Items, int TotalItems)> ListAsync(int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns the image with the given id, tracked for changes, or <see langword="null"/>.</summary>
    Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns the existing images among the given ids, read-only.</summary>
    Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>Whether any cover, logo, photo or album item points to the image.</summary>
    Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Marks a new image to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(MediaAsset mediaAsset);

    /// <summary>Marks an image to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(MediaAsset mediaAsset);
}
