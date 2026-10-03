using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class ClientRepository(AppDbContext context) : IClientRepository
{
    public async Task<IReadOnlyList<Client>> ListAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        var query = context.Clients.AsNoTracking();

        if (onlyActive)
        {
            query = query.Where(client => client.IsActive);
        }

        return await query
            .OrderBy(client => client.DisplayOrder)
            .ThenBy(client => client.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Clients.FirstOrDefaultAsync(client => client.Id == id, cancellationToken);

    public void Add(Client client) => context.Clients.Add(client);

    public void Remove(Client client) => context.Clients.Remove(client);
}
