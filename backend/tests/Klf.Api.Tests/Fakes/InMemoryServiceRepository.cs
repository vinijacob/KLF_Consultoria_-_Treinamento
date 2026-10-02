using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Api.Tests.Fakes;

public sealed class InMemoryServiceRepository : IServiceRepository, IUnitOfWork
{
    public List<Service> Services { get; } = [];

    public Task<IReadOnlyList<Service>> ListAsync(bool onlyActive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Service>>([.. Services.Where(s => !onlyActive || s.IsActive).OrderBy(s => s.DisplayOrder)]);

    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Services.SingleOrDefault(s => s.Id == id));

    public Task<Service?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken) =>
        Task.FromResult(Services.SingleOrDefault(s => s.Slug == slug && s.IsActive));

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        Task.FromResult(Services.Any(s => s.Slug == slug && s.Id != excludingId));

    public void Add(Service service) => Services.Add(service);

    public void Remove(Service service) => Services.Remove(service);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
}
