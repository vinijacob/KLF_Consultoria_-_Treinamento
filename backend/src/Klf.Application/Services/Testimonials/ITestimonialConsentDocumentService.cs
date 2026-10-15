using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Testimonials;

namespace Klf.Application.Services.Testimonials;

/// <summary>Keeps the signed consent term (PDF) of a testimonial in private storage: attach, download and remove.</summary>
public interface ITestimonialConsentDocumentService
{
    /// <summary>Largest consent term accepted, in bytes (10 MB).</summary>
    const long MaxFileBytes = 10 * 1024 * 1024;

    /// <summary>Attaches the signed term, replacing the previous file.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The testimonial does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">The file is empty, too large or not a PDF.</exception>
    Task<TestimonialResponse> AttachAsync(Guid testimonialId, UploadConsentDocumentRequest request, CancellationToken cancellationToken);

    /// <summary>Returns the signed term to download.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The testimonial does not exist or has no term attached.</exception>
    Task<FileDownload> DownloadAsync(Guid testimonialId, CancellationToken cancellationToken);

    /// <summary>Removes the signed term and deletes the file.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The testimonial does not exist.</exception>
    Task<TestimonialResponse> RemoveAsync(Guid testimonialId, CancellationToken cancellationToken);
}
