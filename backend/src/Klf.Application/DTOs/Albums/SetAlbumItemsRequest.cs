namespace Klf.Application.DTOs.Albums;

/// <summary>The complete list of images of an album, in display order. Images left out of the list are removed from the album.</summary>
/// <param name="Items">Images in the desired order (up to 200).</param>
public sealed record SetAlbumItemsRequest(IReadOnlyList<AlbumItemRequest> Items);

/// <summary>One image of an album.</summary>
/// <param name="MediaAssetId">Id of an uploaded image.</param>
/// <param name="Caption">Caption shown under the image (up to 300 characters); optional.</param>
public sealed record AlbumItemRequest(Guid MediaAssetId, string? Caption);
