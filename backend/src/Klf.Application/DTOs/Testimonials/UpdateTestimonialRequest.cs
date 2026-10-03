namespace Klf.Application.DTOs.Testimonials;

/// <summary>Data to replace a testimonial. The consent revocation is not editable here: use the revoke endpoint.</summary>
/// <param name="AuthorName">Who gave the testimonial (up to 120 characters).</param>
/// <param name="AuthorRole">Job title of the author (up to 120 characters); optional.</param>
/// <param name="CompanyName">Company or store of the author (up to 200 characters); optional.</param>
/// <param name="Quote">The testimonial text, plain text (up to 1000 characters).</param>
/// <param name="PhotoId">Photo of the author; optional, and only allowed when <c>ConsentCoversImage</c> is true.</param>
/// <param name="ConsentGivenAt">When the author signed the consent term; required to publish; cannot be in the future.</param>
/// <param name="ConsentCoversImage">Whether the consent also covers the use of the author's image.</param>
/// <param name="IsPublished">Whether the testimonial is shown on the public site; requires <c>ConsentGivenAt</c>.</param>
/// <param name="DisplayOrder">Position in the public list; lower numbers come first.</param>
public sealed record UpdateTestimonialRequest(
    string AuthorName,
    string? AuthorRole,
    string? CompanyName,
    string Quote,
    Guid? PhotoId,
    DateTimeOffset? ConsentGivenAt,
    bool ConsentCoversImage,
    bool IsPublished,
    int DisplayOrder) : ITestimonialFields;
