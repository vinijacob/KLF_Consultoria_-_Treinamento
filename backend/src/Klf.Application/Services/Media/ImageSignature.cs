namespace Klf.Application.Services.Media;

/// <summary>Detects the real type of an image from its first bytes.</summary>
internal static class ImageSignature
{
    public sealed record ImageType(string ContentType, string Extension);

    public static ImageType? Detect(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return new ImageType("image/jpeg", ".jpg");
        }

        if (bytes.Length >= 8 && bytes[..8].SequenceEqual((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return new ImageType("image/png", ".png");
        }

        if (bytes.Length >= 12
            && bytes[..4].SequenceEqual("RIFF"u8)
            && bytes[8..12].SequenceEqual("WEBP"u8))
        {
            return new ImageType("image/webp", ".webp");
        }

        return null;
    }
}
