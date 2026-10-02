namespace Klf.Application.DTOs.Settings;

/// <summary>Contact data shown on the site and used by the floating WhatsApp button.</summary>
/// <param name="Whatsapp">Digits only, with country and area code (e.g. <c>5592999999999</c>).</param>
/// <param name="Email">Public contact e-mail.</param>
/// <param name="Phone">Phone number (digits, spaces, parentheses, hyphen and <c>+</c>).</param>
/// <param name="Address">Address (up to 300 characters).</param>
/// <param name="MapUrl">Link to the map; must start with <c>https://</c>.</param>
public sealed record ContactSettings(
    string? Whatsapp,
    string? Email,
    string? Phone,
    string? Address,
    string? MapUrl);
