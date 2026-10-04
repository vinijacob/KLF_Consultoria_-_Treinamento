namespace Klf.Application.DTOs.Media;

/// <summary>An image sent by the admin panel, already read from the HTTP request.</summary>
/// <param name="Content">File content.</param>
/// <param name="FileName">File name as the browser sent it; used only to recognize the image in the panel.</param>
/// <param name="Length">File size in bytes.</param>
/// <param name="AltText">Text alternative for screen readers (up to 200 characters); optional.</param>
public sealed record UploadMediaRequest(Stream Content, string FileName, long Length, string? AltText);
