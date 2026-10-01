using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
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
            .ToListAsync(cancellationToken);

    public Task<CareerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.CareerEntries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(CareerEntry entry) => context.CareerEntries.Add(entry);

    public void Remove(CareerEntry entry) => context.CareerEntries.Remove(entry);
}
