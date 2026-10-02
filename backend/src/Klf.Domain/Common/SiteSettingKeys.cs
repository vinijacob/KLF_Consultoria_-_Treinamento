namespace Klf.Domain.Common;

/// <summary>
/// Names of the site settings that exist. Each one stores a JSON object with a fixed shape,
/// defined and validated in the Application layer (see <c>DTOs/Settings</c>).
/// </summary>
public static class SiteSettingKeys
{
    /// <summary>Institutional texts: mission, vision, values, differentials and history.</summary>
    public const string About = "about";

    /// <summary>Contact data: WhatsApp, e-mail, phone, address and map link.</summary>
    public const string Contact = "contact";

    /// <summary>Links to the social networks.</summary>
    public const string Social = "social";

    /// <summary>Default SEO: title, description and share image used when a page has none of its own.</summary>
    public const string Seo = "seo";

    /// <summary>Every existing key.</summary>
    public static IReadOnlyList<string> All { get; } = [About, Contact, Social, Seo];
}
