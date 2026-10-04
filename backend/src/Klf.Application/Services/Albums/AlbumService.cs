using Klf.Application.DTOs.Albums;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Interfaces.Storage;
using Klf.Application.Mappings;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Albums;

internal sealed class AlbumService(
    IAlbumRepository repository,
    IMediaAssetRepository mediaRepository,
    IUnitOfWork unitOfWork,
    IFileStorage storage) : IAlbumService
{
    public async Task<IReadOnlyList<AlbumListItemResponse>> ListPublicAsync(CancellationToken cancellationToken)
    {
        var albums = await repository.ListAsync(onlyActive: true, cancellationToken);
        var media = await LoadMediaAsync(albums, cancellationToken);

        return [.. albums.Select(album => album.ToListItem(media, storage))];
    }

    public async Task<PublicAlbumResponse> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var album = await repository.GetActiveBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException("Álbum não encontrado.");
        var media = await LoadMediaAsync([album], cancellationToken);

        return album.ToPublicResponse(media, storage);
    }

    public async Task<IReadOnlyList<AlbumListItemResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var albums = await repository.ListAsync(onlyActive: false, cancellationToken);
        var media = await LoadMediaAsync(albums, cancellationToken);

        return [.. albums.Select(album => album.ToListItem(media, storage))];
    }

    public async Task<AlbumResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var album = await GetOrThrowAsync(id, cancellationToken);

        return await ToResponseAsync(album, cancellationToken);
    }

    public async Task<AlbumResponse> CreateAsync(CreateAlbumRequest request, CancellationToken cancellationToken)
    {
        await EnsureSlugIsFreeAsync(request.Slug, excludingId: null, cancellationToken);
        await EnsureCoverExistsAsync(request.CoverId, cancellationToken);

        var album = new Album(
            request.Title.Trim(),
            request.Slug,
            NullIfBlank(request.Description),
            request.CoverId,
            request.DisplayOrder,
            request.IsActive);

        repository.Add(album);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(album, cancellationToken);
    }

    public async Task<AlbumResponse> UpdateAsync(Guid id, UpdateAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = await GetOrThrowAsync(id, cancellationToken);
        await EnsureSlugIsFreeAsync(request.Slug, excludingId: id, cancellationToken);
        await EnsureCoverExistsAsync(request.CoverId, cancellationToken);

        album.Update(
            request.Title.Trim(),
            request.Slug,
            NullIfBlank(request.Description),
            request.CoverId,
            request.DisplayOrder,
            request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(album, cancellationToken);
    }

    public async Task<AlbumResponse> SetItemsAsync(Guid id, SetAlbumItemsRequest request, CancellationToken cancellationToken)
    {
        var album = await GetOrThrowAsync(id, cancellationToken);

        var ids = request.Items.Select(item => item.MediaAssetId).Distinct().ToList();
        var existing = await mediaRepository.GetByIdsAsync(ids, cancellationToken);

        if (existing.Count != ids.Count)
        {
            throw new ValidationException("Items", "Uma ou mais imagens não foram encontradas. Envie a imagem primeiro.");
        }

        album.SetItems([.. request.Items.Select(item => (item.MediaAssetId, NullIfBlank(item.Caption)))]);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(album, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var album = await GetOrThrowAsync(id, cancellationToken);

        repository.Remove(album);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private async Task<AlbumResponse> ToResponseAsync(Album album, CancellationToken cancellationToken)
    {
        var media = await LoadMediaAsync([album], cancellationToken);

        return album.ToResponse(media, storage);
    }

    private async Task<IReadOnlyDictionary<Guid, MediaAsset>> LoadMediaAsync(
        IEnumerable<Album> albums,
        CancellationToken cancellationToken)
    {
        var ids = albums
            .SelectMany(album => album.Items.Select(item => item.MediaAssetId).Concat(album.CoverId is { } cover ? [cover] : []))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<Guid, MediaAsset>();
        }

        var media = await mediaRepository.GetByIdsAsync(ids, cancellationToken);

        return media.ToDictionary(m => m.Id);
    }

    private async Task<Album> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Álbum", id);

    private async Task EnsureSlugIsFreeAsync(string slug, Guid? excludingId, CancellationToken cancellationToken)
    {
        if (await repository.SlugExistsAsync(slug, excludingId, cancellationToken))
        {
            throw new ConflictException($"Já existe um álbum com o endereço '{slug}'. Escolha outro slug.");
        }
    }

    private async Task EnsureCoverExistsAsync(Guid? coverId, CancellationToken cancellationToken)
    {
        if (coverId is { } id && await mediaRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            throw new ValidationException("CoverId", "Imagem de capa não encontrada. Envie a imagem primeiro.");
        }
    }
}
