namespace Klf.Application.DTOs.Testimonials;

/// <summary>A published testimonial as the public site shows it: no consent data.</summary>
/// <param name="Id">Testimonial identifier.</param>
/// <param name="AuthorName">Who gave the testimonial.</param>
/// <param name="AuthorRole">Job title of the author.</param>
/// <param name="CompanyName">Company or store of the author.</param>
/// <param name="Quote">The testimonial text.</param>
/// <param name="PhotoId">Photo of the author; only present when the consent covers the use of the image.</param>
public sealed record PublicTestimonialResponse(
    Guid Id,
    string AuthorName,
    string? AuthorRole,
    string? CompanyName,
    string Quote,
    Guid? PhotoId);
