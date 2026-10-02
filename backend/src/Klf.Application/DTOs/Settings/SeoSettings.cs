namespace Klf.Application.DTOs.Settings;

/// <summary>Default SEO, used by pages that have no title, description or share image of their own.</summary>
/// <param name="Title">Default title (up to 60 characters).</param>
/// <param name="Description">Default description (up to 160 characters).</param>
/// <param name="ShareImageId">Image shown when a link is shared on WhatsApp or LinkedIn.</param>
public sealed record SeoSettings(
    string? Title,
    string? Description,
    Guid? ShareImageId);
