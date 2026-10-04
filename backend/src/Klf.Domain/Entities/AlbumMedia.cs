using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>An image inside an <see cref="Album"/>, with its caption and position.</summary>
public sealed class AlbumMedia : Entity
{
    /// <summary>Creates an album item.</summary>
    /// <param name="mediaAssetId">The image.</param>
    /// <param name="caption">Caption shown under the image; optional.</param>
    /// <param name="displayOrder">Position in the album; lower numbers come first.</param>
    public AlbumMedia(Guid mediaAssetId, string? caption, int displayOrder)
    {
        MediaAssetId = mediaAssetId;
        Caption = caption;
        DisplayOrder = displayOrder;
    }

    /// <summary>The album this item belongs to.</summary>
    public Guid AlbumId { get; private set; }

    /// <summary>The image.</summary>
    public Guid MediaAssetId { get; private set; }

    /// <summary>Caption shown under the image; optional.</summary>
    public string? Caption { get; private set; }

    /// <summary>Position in the album; lower numbers come first.</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>Changes the caption and the position.</summary>
    /// <param name="caption">Caption shown under the image; optional.</param>
    /// <param name="displayOrder">Position in the album.</param>
    public void Update(string? caption, int displayOrder)
    {
        Caption = caption;
        DisplayOrder = displayOrder;
    }
}
