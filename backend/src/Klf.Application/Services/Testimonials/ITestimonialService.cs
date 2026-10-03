using Klf.Application.DTOs.Testimonials;

namespace Klf.Application.Services.Testimonials;

/// <summary>Manages testimonials and the consent of their authors, for the admin panel and the public site.</summary>
public interface ITestimonialService
{
    /// <summary>Lists the testimonials that are published with valid consent, in display order (public site).</summary>
    Task<IReadOnlyList<PublicTestimonialResponse>> ListPublicAsync(CancellationToken cancellationToken);

    /// <summary>Lists every testimonial, published or not, in display order (admin panel).</summary>
    Task<IReadOnlyList<TestimonialResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns one testimonial with its consent status.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The testimonial does not exist.</exception>
    Task<TestimonialResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a testimonial.</summary>
    /// <exception cref="Domain.Exceptions.ValidationException">The consent is in the future, or a publishing rule is broken.</exception>
    Task<TestimonialResponse> CreateAsync(CreateTestimonialRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces every editable field of a testimonial.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The testimonial does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">The consent is in the future, or a publishing rule is broken.</exception>
    Task<TestimonialResponse> UpdateAsync(Guid id, UpdateTestimonialRequest request, CancellationToken cancellationToken);

    /// <summary>Records that the author withdrew the consent: unpublishes the testimonial for good.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The testimonial does not exist.</exception>
    Task<TestimonialResponse> RevokeConsentAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Soft-deletes a testimonial; it disappears from the site but stays in the database.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The testimonial does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
