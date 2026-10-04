using Klf.Application.DTOs.Albums;
using Klf.Application.DTOs.Media;
using Klf.Application.Interfaces.Storage;
using Klf.Domain.Entities;

namespace Klf.Application.Mappings;

internal static class MediaMappings
{
    public static MediaAssetResponse ToResponse(this MediaAsset media, IFileStorage storage) => new(
        media.Id,
        storage.GetPublicUrl(media.StorageKey),
        media.OriginalFileName,
        media.ContentType,
        media.SizeBytes,
        media.AltText,
        DateTime.SpecifyKind(media.CreatedAt, DateTimeKind.Utc));

    public static AlbumResponse ToResponse(this Album album, IReadOnlyDictionary<Guid, MediaAsset> media, IFileStorage storage) => new(
        album.Id,
        album.Title,
        album.Slug,
        album.Description,
        album.CoverId,
        CoverUrl(album, media, storage),
        album.DisplayOrder,
        album.IsActive,
        [.. album.Items
            .Where(item => media.ContainsKey(item.MediaAssetId))
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new AlbumItemResponse(
                item.MediaAssetId,
                storage.GetPublicUrl(media[item.MediaAssetId].StorageKey),
                media[item.MediaAssetId].AltText,
                item.Caption,
                item.DisplayOrder))]);

    public static AlbumListItemResponse ToListItem(this Album album, IReadOnlyDictionary<Guid, MediaAsset> media, IFileStorage storage) => new(
        album.Id,
        album.Title,
        album.Slug,
        album.Description,
        CoverUrl(album, media, storage),
        album.Items.Count,
        album.DisplayOrder,
        album.IsActive);

    public static PublicAlbumResponse ToPublicResponse(this Album album, IReadOnlyDictionary<Guid, MediaAsset> media, IFileStorage storage) => new(
        album.Id,
        album.Title,
        album.Slug,
        album.Description,
        CoverUrl(album, media, storage),
        [.. album.Items
            .Where(item => media.ContainsKey(item.MediaAssetId))
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new PublicAlbumItemResponse(
                storage.GetPublicUrl(media[item.MediaAssetId].StorageKey),
                media[item.MediaAssetId].AltText,
                item.Caption))]);

    private static string? CoverUrl(Album album, IReadOnlyDictionary<Guid, MediaAsset> media, IFileStorage storage) =>
        album.CoverId is { } coverId && media.TryGetValue(coverId, out var cover)
            ? storage.GetPublicUrl(cover.StorageKey)
            : null;
}
