namespace Klf.Application.DTOs.Settings;

/// <summary>KLF's social networks, in the order the site shows them. The panel can add any network, by name.</summary>
/// <param name="Links">Up to 12 networks, each with a name and an <c>https://</c> link.</param>
public sealed record SocialSettings(IReadOnlyList<SocialLink>? Links)
{
    /// <summary>Most networks.</summary>
    public const int MaxLinks = 12;
}

/// <summary>One social network.</summary>
/// <param name="Name">Name shown on the site (e.g. Instagram, TikTok; up to 40 characters).</param>
/// <param name="Url">Profile link, starting with <c>https://</c> (up to 300 characters).</param>
public sealed record SocialLink(string Name, string Url);
