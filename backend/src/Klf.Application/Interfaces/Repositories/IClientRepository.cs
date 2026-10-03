using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="Client"/>. Soft-deleted clients are never returned.</summary>
public interface IClientRepository
{
    /// <summary>
    /// Returns the clients ordered by <see cref="Client.DisplayOrder"/> (then name), read-only.
    /// </summary>
    /// <param name="onlyActive">When <see langword="true"/>, skips inactive clients (public site).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<Client>> ListAsync(bool onlyActive, CancellationToken cancellationToken);

    /// <summary>Returns the client with the given id, tracked for changes, or <see langword="null"/>.</summary>
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Marks a new client to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Client client);

    /// <summary>Marks a client to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(Client client);
}
