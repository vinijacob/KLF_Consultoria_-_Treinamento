namespace Klf.Application.DTOs.Settings;

/// <summary>Institutional texts of the "Sobre a KLF" page. Plain text only: the site shows it as text, never as HTML.</summary>
/// <param name="Mission">Mission (up to 1000 characters).</param>
/// <param name="Vision">Vision (up to 1000 characters).</param>
/// <param name="Values">Company values, one item each (up to 20 items of 200 characters).</param>
/// <param name="Differentials">Differentials, one item each (up to 20 items of 200 characters).</param>
/// <param name="History">Company history (up to 5000 characters).</param>
public sealed record AboutSettings(
    string? Mission,
    string? Vision,
    IReadOnlyList<string>? Values,
    IReadOnlyList<string>? Differentials,
    string? History);
