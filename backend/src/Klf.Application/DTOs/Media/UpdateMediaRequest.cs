namespace Klf.Application.DTOs.Media;

/// <summary>Data to change an uploaded image. Only the text alternative can change; to replace the file, upload a new one.</summary>
/// <param name="AltText">Text alternative for screen readers (up to 200 characters); empty clears it.</param>
public sealed record UpdateMediaRequest(string? AltText);
