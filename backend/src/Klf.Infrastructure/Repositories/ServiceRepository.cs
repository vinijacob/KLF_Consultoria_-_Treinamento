using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class ServiceRepository(AppDbContext context) : IServiceRepository
{
    public async Task<IReadOnlyList<Service>> ListAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        var query = context.Services.AsNoTracking();

        if (onlyActive)
        {
            query = query.Where(service => service.IsActive);
        }

        return await query
            .OrderBy(service => service.DisplayOrder)
            .ThenBy(service => service.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Services.FirstOrDefaultAsync(service => service.Id == id, cancellationToken);

    public Task<Service?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken) =>
        context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(service => service.Slug == slug && service.IsActive, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Services.AnyAsync(service => service.Slug == slug && service.Id != excludingId, cancellationToken);

    public void Add(Service service) => context.Services.Add(service);

    public void Remove(Service service) => context.Services.Remove(service);
}
