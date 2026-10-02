namespace Klf.Application.DTOs.Settings;

/// <summary>Links to KLF's social networks. Every link must start with <c>https://</c>.</summary>
/// <param name="Instagram">Instagram profile link.</param>
/// <param name="Linkedin">LinkedIn page link.</param>
/// <param name="Facebook">Facebook page link.</param>
/// <param name="Youtube">YouTube channel link.</param>
public sealed record SocialSettings(
    string? Instagram,
    string? Linkedin,
    string? Facebook,
    string? Youtube);
