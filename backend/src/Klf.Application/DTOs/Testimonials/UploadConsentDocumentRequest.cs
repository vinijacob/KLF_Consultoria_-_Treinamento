namespace Klf.Application.DTOs.Testimonials;

/// <summary>A signed consent term (PDF) sent by the admin panel, already read from the HTTP request.</summary>
/// <param name="Content">File content.</param>
/// <param name="FileName">File name as the browser sent it; used only to recognize the file in the panel.</param>
/// <param name="Length">File size in bytes.</param>
public sealed record UploadConsentDocumentRequest(Stream Content, string FileName, long Length);
