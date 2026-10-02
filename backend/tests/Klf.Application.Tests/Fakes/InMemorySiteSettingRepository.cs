using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Fakes;

internal sealed class InMemorySiteSettingRepository : ISiteSettingRepository, IUnitOfWork
{
    public List<SiteSetting> Settings { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<SiteSetting>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<SiteSetting>>([.. Settings]);

    public Task<SiteSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken) =>
        Task.FromResult(Settings.SingleOrDefault(s => s.Key == key));

    public void Add(SiteSetting setting) => Settings.Add(setting);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}
