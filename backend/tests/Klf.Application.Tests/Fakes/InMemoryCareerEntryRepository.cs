using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Fakes;

internal sealed class InMemoryCareerEntryRepository : ICareerEntryRepository, IUnitOfWork
{
    public List<CareerEntry> Entries { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<CareerEntry>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CareerEntry>>([.. Entries.OrderBy(e => e.EntryType).ThenBy(e => e.DisplayOrder)]);

    public Task<CareerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Entries.SingleOrDefault(e => e.Id == id));

    public void Add(CareerEntry entry) => Entries.Add(entry);

    public void Remove(CareerEntry entry) => Entries.Remove(entry);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}
