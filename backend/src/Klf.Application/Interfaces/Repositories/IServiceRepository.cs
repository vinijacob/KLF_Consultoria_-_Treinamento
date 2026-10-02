using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="Service"/>. Soft-deleted services are never returned.</summary>
public interface IServiceRepository
{
    /// <summary>
    /// Returns the services ordered by <see cref="Service.DisplayOrder"/> (then title), read-only.
    /// </summary>
    /// <param name="onlyActive">When <see langword="true"/>, skips inactive services (public site).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<Service>> ListAsync(bool onlyActive, CancellationToken cancellationToken);

    /// <summary>Returns the service with the given id, tracked for changes, or <see langword="null"/>.</summary>
    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns the active service with the given slug, read-only, or <see langword="null"/>.</summary>
    Task<Service?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Whether another service already uses the slug; <paramref name="excludingId"/> skips the service being edited.</summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken);

    /// <summary>Marks a new service to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Service service);

    /// <summary>Marks a service to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(Service service);
}
