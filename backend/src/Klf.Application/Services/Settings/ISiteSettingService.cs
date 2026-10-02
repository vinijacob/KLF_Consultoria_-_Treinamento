using System.Text.Json;

namespace Klf.Application.Services.Settings;

/// <summary>Reads and saves the site settings (see <c>SiteSettingKeys</c>).</summary>
public interface ISiteSettingService
{
    /// <summary>Returns every configured setting as <c>{ "about": {...}, "contact": {...} }</c>; keys never saved are absent.</summary>
    Task<IReadOnlyDictionary<string, JsonElement>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns one setting.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The key does not exist or was never saved.</exception>
    Task<JsonElement> GetAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or replaces a setting. The value is checked against the shape of the key: unknown fields,
    /// wrong types and invalid values are rejected.
    /// </summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The key does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">The value does not match the expected shape or has invalid fields.</exception>
    Task<JsonElement> UpsertAsync(string key, JsonElement value, CancellationToken cancellationToken);
}
