using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Media;

namespace Klf.Application.Services.Media;

/// <summary>Manages uploaded images.</summary>
public interface IMediaService
{
    /// <summary>Largest accepted file, in bytes.</summary>
    const long MaxFileBytes = 5 * 1024 * 1024;

    /// <summary>Lists the uploaded images, newest first.</summary>
    Task<PagedResponse<MediaAssetResponse>> ListAsync(PagedRequest request, CancellationToken cancellationToken);

    /// <summary>Returns one image.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The image does not exist.</exception>
    Task<MediaAssetResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns the public address of an image (public site).</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The image does not exist.</exception>
    Task<string> GetPublicUrlAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Validates and stores an image. The type is detected from the file content (JPEG, PNG or WebP), never from its name or declared type.
    /// </summary>
    /// <exception cref="Domain.Exceptions.ValidationException">The file is empty, too large, not a supported image, or the text alternative is too long.</exception>
    Task<MediaAssetResponse> UploadAsync(UploadMediaRequest request, CancellationToken cancellationToken);

    /// <summary>Changes the text alternative of an image.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The image does not exist.</exception>
    Task<MediaAssetResponse> UpdateAsync(Guid id, UpdateMediaRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes an image and its file.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The image does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">A cover, logo, photo or album still uses the image.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
