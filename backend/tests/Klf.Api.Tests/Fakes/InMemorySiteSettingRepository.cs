using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Api.Tests.Fakes;

public sealed class InMemorySiteSettingRepository : ISiteSettingRepository, IUnitOfWork
{
    public List<SiteSetting> Settings { get; } = [];

    public Task<IReadOnlyList<SiteSetting>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<SiteSetting>>([.. Settings]);

    public Task<SiteSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken) =>
        Task.FromResult(Settings.SingleOrDefault(s => s.Key == key));

    public void Add(SiteSetting setting) => Settings.Add(setting);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
}
