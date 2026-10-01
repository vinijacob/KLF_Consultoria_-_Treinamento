using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="CareerEntry"/>. Soft-deleted entries are never returned.</summary>
public interface ICareerEntryRepository
{
    /// <summary>Returns every entry ordered by type and then by <see cref="CareerEntry.DisplayOrder"/>, read-only.</summary>
    Task<IReadOnlyList<CareerEntry>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns the entry with the given id, tracked for changes, or <see langword="null"/> if it does not exist.
    /// </summary>
    Task<CareerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Marks a new entry to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(CareerEntry entry);

    /// <summary>Marks an entry to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(CareerEntry entry);
}
