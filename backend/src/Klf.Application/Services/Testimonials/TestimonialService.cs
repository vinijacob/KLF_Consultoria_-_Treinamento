using Klf.Application.DTOs.Testimonials;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
using Klf.Application.Services.Media;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Testimonials;

internal sealed class TestimonialService(
    ITestimonialRepository repository,
    IMediaAssetRepository mediaRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ITestimonialService
{
    public async Task<IReadOnlyList<PublicTestimonialResponse>> ListPublicAsync(CancellationToken cancellationToken)
    {
        var testimonials = await repository.ListAsync(onlyVisible: true, cancellationToken);

        return [.. testimonials.Select(testimonial => testimonial.ToPublicResponse())];
    }

    public async Task<IReadOnlyList<TestimonialResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var testimonials = await repository.ListAsync(onlyVisible: false, cancellationToken);

        return [.. testimonials.Select(testimonial => testimonial.ToResponse())];
    }

    public async Task<TestimonialResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var testimonial = await GetOrThrowAsync(id, cancellationToken);

        return testimonial.ToResponse();
    }

    public async Task<TestimonialResponse> CreateAsync(CreateTestimonialRequest request, CancellationToken cancellationToken)
    {
        await MediaReference.EnsureExistsAsync(mediaRepository, request.PhotoId, "PhotoId", cancellationToken);

        var testimonial = new Testimonial(
            request.AuthorName.Trim(),
            NullIfBlank(request.AuthorRole),
            NullIfBlank(request.CompanyName),
            request.Quote.Trim(),
            request.PhotoId,
            ConsentDate(request.ConsentGivenAt),
            request.ConsentCoversImage,
            request.IsPublished,
            request.DisplayOrder);

        repository.Add(testimonial);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return testimonial.ToResponse();
    }

    public async Task<TestimonialResponse> UpdateAsync(Guid id, UpdateTestimonialRequest request, CancellationToken cancellationToken)
    {
        var testimonial = await GetOrThrowAsync(id, cancellationToken);
        await MediaReference.EnsureExistsAsync(mediaRepository, request.PhotoId, "PhotoId", cancellationToken);

        testimonial.Update(
            request.AuthorName.Trim(),
            NullIfBlank(request.AuthorRole),
            NullIfBlank(request.CompanyName),
            request.Quote.Trim(),
            request.PhotoId,
            ConsentDate(request.ConsentGivenAt),
            request.ConsentCoversImage,
            request.IsPublished,
            request.DisplayOrder);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return testimonial.ToResponse();
    }

    public async Task<TestimonialResponse> RevokeConsentAsync(Guid id, CancellationToken cancellationToken)
    {
        var testimonial = await GetOrThrowAsync(id, cancellationToken);

        testimonial.RevokeConsent(UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return testimonial.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var testimonial = await GetOrThrowAsync(id, cancellationToken);

        repository.Remove(testimonial);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private DateTime? ConsentDate(DateTimeOffset? consentGivenAt)
    {
        var utc = consentGivenAt?.UtcDateTime;

        if (utc > UtcNow)
        {
            throw new ValidationException("ConsentGivenAt", "A data do consentimento não pode estar no futuro.");
        }

        return utc;
    }

    private async Task<Testimonial> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Depoimento", id);
}
