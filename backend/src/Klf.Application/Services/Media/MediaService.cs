using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Media;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Interfaces.Storage;
using Klf.Application.Mappings;
using Klf.Application.Validators.Media;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Media;

internal sealed class MediaService(
    IMediaAssetRepository repository,
    IUnitOfWork unitOfWork,
    IFileStorage storage,
    TimeProvider timeProvider) : IMediaService
{
    private const int FileNameMaxLength = 200;

    public async Task<PagedResponse<MediaAssetResponse>> ListAsync(PagedRequest request, CancellationToken cancellationToken)
    {
        var (items, total) = await repository.ListAsync(request.Skip, request.PageSize, cancellationToken);

        return new PagedResponse<MediaAssetResponse>(
            [.. items.Select(media => media.ToResponse(storage))],
            request.Page,
            request.PageSize,
            total);
    }

    public async Task<MediaAssetResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var media = await GetOrThrowAsync(id, cancellationToken);

        return media.ToResponse(storage);
    }

    public async Task<string> GetPublicUrlAsync(Guid id, CancellationToken cancellationToken)
    {
        var media = await GetOrThrowAsync(id, cancellationToken);

        return storage.GetPublicUrl(media.StorageKey);
    }

    public async Task<MediaAssetResponse> UploadAsync(UploadMediaRequest request, CancellationToken cancellationToken)
    {
        var altText = NullIfBlank(request.AltText);

        if (altText?.Length > UpdateMediaRequestValidator.AltTextMaxLength)
        {
            throw new ValidationException("AltText", $"O texto alternativo pode ter no máximo {UpdateMediaRequestValidator.AltTextMaxLength} caracteres.");
        }

        if (request.Length == 0)
        {
            throw new ValidationException("File", "O arquivo está vazio.");
        }

        if (request.Length > IMediaService.MaxFileBytes)
        {
            throw new ValidationException("File", "A imagem pode ter no máximo 5 MB.");
        }

        using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);

        if (buffer.Length > IMediaService.MaxFileBytes)
        {
            throw new ValidationException("File", "A imagem pode ter no máximo 5 MB.");
        }

        var image = ImageSignature.Detect(buffer.GetBuffer().AsSpan(0, (int)buffer.Length))
            ?? throw new ValidationException("File", "Formato não suportado. Envie uma imagem JPEG, PNG ou WebP.");

        var now = timeProvider.GetUtcNow();
        var key = $"media/{now:yyyy/MM}/{Guid.CreateVersion7()}{image.Extension}";

        buffer.Position = 0;
        await storage.SaveAsync(key, buffer, image.ContentType, cancellationToken);

        var media = new MediaAsset(key, CleanFileName(request.FileName), image.ContentType, buffer.Length, altText);

        try
        {
            repository.Add(media);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storage.DeleteAsync(key, CancellationToken.None);
            throw;
        }

        return media.ToResponse(storage);
    }

    public async Task<MediaAssetResponse> UpdateAsync(Guid id, UpdateMediaRequest request, CancellationToken cancellationToken)
    {
        var media = await GetOrThrowAsync(id, cancellationToken);

        media.UpdateAltText(NullIfBlank(request.AltText));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return media.ToResponse(storage);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var media = await GetOrThrowAsync(id, cancellationToken);

        if (await repository.IsReferencedAsync(id, cancellationToken))
        {
            throw new ConflictException("Esta imagem está em uso (capa, logo, foto ou álbum). Remova-a de lá antes de excluir.");
        }

        repository.Remove(media);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await storage.DeleteAsync(media.StorageKey, cancellationToken);
    }

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static string CleanFileName(string fileName)
    {
        var name = Path.GetFileName(fileName.Replace('\\', '/')).Trim();

        if (name.Length == 0)
        {
            name = "imagem";
        }

        return name.Length > FileNameMaxLength ? name[..FileNameMaxLength] : name;
    }

    private async Task<MediaAsset> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Imagem", id);
}
