namespace Klf.Application.DTOs.Testimonials;

/// <summary>The signed consent term attached to a testimonial (the file itself is downloaded from its own route).</summary>
/// <param name="FileName">File name as it was uploaded.</param>
/// <param name="SizeBytes">File size in bytes.</param>
/// <param name="UploadedAt">When the file was attached.</param>
public sealed record ConsentDocumentResponse(string FileName, long SizeBytes, DateTimeOffset UploadedAt);
