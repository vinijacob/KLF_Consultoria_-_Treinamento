using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Api.Tests.Fakes;

public sealed class InMemoryClientRepository : IClientRepository, IUnitOfWork
{
    public List<Client> Clients { get; } = [];

    public Task<IReadOnlyList<Client>> ListAsync(bool onlyActive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Client>>([.. Clients.Where(x => !onlyActive || x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)]);

    public Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Clients.SingleOrDefault(x => x.Id == id));

    public void Add(Client client) => Clients.Add(client);

    public void Remove(Client client) => Clients.Remove(client);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
}
