using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class CareerEntryRepository(AppDbContext context) : ICareerEntryRepository
{
    public async Task<IReadOnlyList<CareerEntry>> ListAsync(CancellationToken cancellationToken) =>
        await context.CareerEntries
            .AsNoTracking()
            .OrderBy(x => x.EntryType)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CareerEntry>> ListInOrderAsync(CareerEntryType entryType, CancellationToken cancellationToken) =>
        await context.CareerEntries
            .Where(x => x.EntryType == entryType)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

    public Task<CareerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.CareerEntries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(CareerEntry entry) => context.CareerEntries.Add(entry);

    public void Remove(CareerEntry entry) => context.CareerEntries.Remove(entry);
}
