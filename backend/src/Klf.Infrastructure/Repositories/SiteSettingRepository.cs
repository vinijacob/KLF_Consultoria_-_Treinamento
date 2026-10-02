using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class SiteSettingRepository(AppDbContext context) : ISiteSettingRepository
{
    public async Task<IReadOnlyList<SiteSetting>> ListAsync(CancellationToken cancellationToken) =>
        await context.SiteSettings.AsNoTracking().OrderBy(setting => setting.Key).ToListAsync(cancellationToken);

    public Task<SiteSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken) =>
        context.SiteSettings.FirstOrDefaultAsync(setting => setting.Key == key, cancellationToken);

    public void Add(SiteSetting setting) => context.SiteSettings.Add(setting);
}
