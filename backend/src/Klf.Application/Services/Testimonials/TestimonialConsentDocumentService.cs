using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Testimonials;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Interfaces.Storage;
using Klf.Application.Mappings;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Testimonials;

internal sealed class TestimonialConsentDocumentService(
    ITestimonialRepository repository,
    IUnitOfWork unitOfWork,
    IPrivateFileStorage storage,
    TimeProvider timeProvider) : ITestimonialConsentDocumentService
{
    private const string PdfContentType = "application/pdf";
    private const int FileNameMaxLength = 200;

    public async Task<TestimonialResponse> AttachAsync(Guid testimonialId, UploadConsentDocumentRequest request, CancellationToken cancellationToken)
    {
        var testimonial = await GetOrThrowAsync(testimonialId, cancellationToken);

        if (request.Length == 0)
        {
            throw new ValidationException("File", "O arquivo está vazio.");
        }

        if (request.Length > ITestimonialConsentDocumentService.MaxFileBytes)
        {
            throw new ValidationException("File", "O termo pode ter no máximo 10 MB.");
        }

        using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);

        if (buffer.Length > ITestimonialConsentDocumentService.MaxFileBytes)
        {
            throw new ValidationException("File", "O termo pode ter no máximo 10 MB.");
        }

        if (!buffer.GetBuffer().AsSpan(0, (int)buffer.Length).StartsWith("%PDF-"u8))
        {
            throw new ValidationException("File", "Envie o termo em PDF.");
        }

        var key = $"consent/{testimonial.Id}/{Guid.CreateVersion7()}.pdf";
        buffer.Position = 0;
        await storage.SaveAsync(key, buffer, PdfContentType, cancellationToken);

        string? previous;

        try
        {
            previous = testimonial.AttachConsentDocument(key, CleanFileName(request.FileName), buffer.Length, timeProvider.GetUtcNow().UtcDateTime);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storage.DeleteAsync(key, CancellationToken.None);
            throw;
        }

        if (previous is not null)
        {
            await storage.DeleteAsync(previous, CancellationToken.None);
        }

        return testimonial.ToResponse();
    }

    public async Task<FileDownload> DownloadAsync(Guid testimonialId, CancellationToken cancellationToken)
    {
        var testimonial = await GetOrThrowAsync(testimonialId, cancellationToken);

        var content = testimonial.ConsentDocumentKey is { } key ? await storage.ReadAsync(key, cancellationToken) : null;

        return content is null
            ? throw new NotFoundException("Este depoimento não tem termo de consentimento anexado.")
            : new FileDownload(content, PdfContentType, testimonial.ConsentDocumentName ?? "termo.pdf");
    }

    public async Task<TestimonialResponse> RemoveAsync(Guid testimonialId, CancellationToken cancellationToken)
    {
        var testimonial = await GetOrThrowAsync(testimonialId, cancellationToken);

        var previous = testimonial.RemoveConsentDocument();

        if (previous is not null)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await storage.DeleteAsync(previous, CancellationToken.None);
        }

        return testimonial.ToResponse();
    }

    private static string CleanFileName(string fileName)
    {
        var name = Path.GetFileName(fileName.Replace('\\', '/')).Trim();

        if (name.Length == 0)
        {
            name = "termo.pdf";
        }

        return name.Length > FileNameMaxLength ? name[..FileNameMaxLength] : name;
    }

    private async Task<Testimonial> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Depoimento", id);
}
