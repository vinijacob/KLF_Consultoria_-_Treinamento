using Klf.Application.DTOs.Testimonials;
using Klf.Domain.Entities;

namespace Klf.Application.Mappings;

internal static class TestimonialMappings
{
    public static TestimonialResponse ToResponse(this Testimonial testimonial) => new(
        testimonial.Id,
        testimonial.AuthorName,
        testimonial.AuthorRole,
        testimonial.CompanyName,
        testimonial.Quote,
        testimonial.PhotoId,
        Utc(testimonial.ConsentGivenAt),
        testimonial.ConsentCoversImage,
        Utc(testimonial.ConsentRevokedAt),
        testimonial.IsPublished,
        testimonial.DisplayOrder,
        testimonial.ConsentDocumentKey is null
            ? null
            : new ConsentDocumentResponse(
                testimonial.ConsentDocumentName ?? "termo.pdf",
                testimonial.ConsentDocumentSize ?? 0,
                Utc(testimonial.ConsentDocumentUploadedAt) ?? default));

    public static PublicTestimonialResponse ToPublicResponse(this Testimonial testimonial) => new(
        testimonial.Id,
        testimonial.AuthorName,
        testimonial.AuthorRole,
        testimonial.CompanyName,
        testimonial.Quote,
        testimonial.ConsentCoversImage ? testimonial.PhotoId : null);

    private static DateTimeOffset? Utc(DateTime? value) =>
        value is null ? null : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
}
