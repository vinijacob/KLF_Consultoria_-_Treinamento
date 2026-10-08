namespace Klf.Application.DTOs.Common;

/// <summary>A generated file to download.</summary>
/// <param name="Content">File bytes.</param>
/// <param name="ContentType">Media type, e.g. <c>image/png</c>.</param>
/// <param name="FileName">Suggested file name.</param>
public sealed record FileDownload(byte[] Content, string ContentType, string FileName);
