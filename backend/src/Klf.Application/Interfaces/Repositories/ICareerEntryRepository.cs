using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="CareerEntry"/>. Soft-deleted entries are never returned.</summary>
public interface ICareerEntryRepository
{
    /// <summary>Returns every entry ordered by type, then by <see cref="CareerEntry.DisplayOrder"/> and title, read-only.</summary>
    Task<IReadOnlyList<CareerEntry>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns the entry with the given id, tracked for changes, or <see langword="null"/> if it does not exist.
    /// </summary>
    Task<CareerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Returns every entry of one type in the order of the panel list (position, then name), tracked for changes, so their
    /// positions can be renumbered (see <see cref="Klf.Domain.Common.DisplayOrdering"/>).
    /// </summary>
    Task<IReadOnlyList<CareerEntry>> ListInOrderAsync(CareerEntryType entryType, CancellationToken cancellationToken);

    /// <summary>Marks a new entry to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(CareerEntry entry);

    /// <summary>Marks an entry to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(CareerEntry entry);
}
