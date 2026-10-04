namespace Klf.Application.DTOs.Media;

/// <summary>An uploaded image, for the admin panel.</summary>
/// <param name="Id">Image identifier, used by covers, logos, photos and albums.</param>
/// <param name="Url">Public address of the file.</param>
/// <param name="OriginalFileName">File name that was uploaded.</param>
/// <param name="ContentType">Image type (<c>image/jpeg</c>, <c>image/png</c> or <c>image/webp</c>).</param>
/// <param name="SizeBytes">File size in bytes.</param>
/// <param name="AltText">Text alternative for screen readers.</param>
/// <param name="CreatedAt">Upload time (UTC).</param>
public sealed record MediaAssetResponse(
    Guid Id,
    string Url,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string? AltText,
    DateTime CreatedAt);
