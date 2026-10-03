namespace Klf.Application.DTOs.Testimonials;

/// <summary>A testimonial with every field, including the consent status, for the admin panel.</summary>
/// <param name="Id">Testimonial identifier.</param>
/// <param name="AuthorName">Who gave the testimonial.</param>
/// <param name="AuthorRole">Job title of the author.</param>
/// <param name="CompanyName">Company or store of the author.</param>
/// <param name="Quote">The testimonial text.</param>
/// <param name="PhotoId">Photo of the author.</param>
/// <param name="ConsentGivenAt">When the author signed the consent term.</param>
/// <param name="ConsentCoversImage">Whether the consent also covers the use of the author's image.</param>
/// <param name="ConsentRevokedAt">When the author revoked the consent; empty while it stands.</param>
/// <param name="IsPublished">Whether the testimonial is shown on the public site.</param>
/// <param name="DisplayOrder">Position in the public list.</param>
public sealed record TestimonialResponse(
    Guid Id,
    string AuthorName,
    string? AuthorRole,
    string? CompanyName,
    string Quote,
    Guid? PhotoId,
    DateTimeOffset? ConsentGivenAt,
    bool ConsentCoversImage,
    DateTimeOffset? ConsentRevokedAt,
    bool IsPublished,
    int DisplayOrder);
