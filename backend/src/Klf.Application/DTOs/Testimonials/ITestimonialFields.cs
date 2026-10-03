namespace Klf.Application.DTOs.Testimonials;

/// <summary>Editable fields shared by the create and update requests, so both use the same validation rules.</summary>
public interface ITestimonialFields
{
    /// <summary>Who gave the testimonial.</summary>
    string AuthorName { get; }

    /// <summary>Job title of the author.</summary>
    string? AuthorRole { get; }

    /// <summary>Company or store of the author.</summary>
    string? CompanyName { get; }

    /// <summary>The testimonial text, plain text.</summary>
    string Quote { get; }

    /// <summary>Photo of the author.</summary>
    Guid? PhotoId { get; }

    /// <summary>When the author signed the consent term.</summary>
    DateTimeOffset? ConsentGivenAt { get; }

    /// <summary>Whether the consent also covers the use of the author's image.</summary>
    bool ConsentCoversImage { get; }

    /// <summary>Whether the testimonial is shown on the public site.</summary>
    bool IsPublished { get; }

    /// <summary>Position in the public list.</summary>
    int DisplayOrder { get; }
}
