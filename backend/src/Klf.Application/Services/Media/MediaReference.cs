using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Media;

internal static class MediaReference
{
    public static async Task EnsureExistsAsync(
        IMediaAssetRepository repository,
        Guid? mediaAssetId,
        string field,
        CancellationToken cancellationToken)
    {
        if (mediaAssetId is { } id && await repository.GetByIdAsync(id, cancellationToken) is null)
        {
            throw new ValidationException(field, "Imagem não encontrada. Envie a imagem primeiro.");
        }
    }
}
