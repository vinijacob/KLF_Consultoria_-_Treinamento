using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>
/// A named group of site settings (see <see cref="SiteSettingKeys"/>) stored as one JSON object.
/// There is at most one row per key; saving a key again replaces its value.
/// </summary>
public sealed class SiteSetting : Entity
{
    /// <summary>Creates a setting.</summary>
    /// <param name="key">One of <see cref="SiteSettingKeys"/>.</param>
    /// <param name="valueJson">The value as a JSON object, already validated and normalized.</param>
    public SiteSetting(string key, string valueJson)
    {
        Key = key;
        ValueJson = valueJson;
    }

    /// <summary>Unique name of the setting.</summary>
    public string Key { get; private set; }

    /// <summary>The value as a JSON object.</summary>
    public string ValueJson { get; private set; }

    /// <summary>Replaces the value.</summary>
    /// <param name="valueJson">The new value as a JSON object, already validated and normalized.</param>
    public void Update(string valueJson)
    {
        ValueJson = valueJson;
    }
}
