using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="SiteSetting"/>.</summary>
public interface ISiteSettingRepository
{
    /// <summary>Returns every configured setting, read-only.</summary>
    Task<IReadOnlyList<SiteSetting>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns the setting with the given key, tracked for changes, or <see langword="null"/> if it was never saved.</summary>
    Task<SiteSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken);

    /// <summary>Marks a new setting to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(SiteSetting setting);
}
