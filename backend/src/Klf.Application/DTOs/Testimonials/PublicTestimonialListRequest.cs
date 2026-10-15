namespace Klf.Application.DTOs.Testimonials;

/// <summary>Options for the public testimonial list (query string: <c>?latest=5</c>).</summary>
public sealed record PublicTestimonialListRequest
{
    /// <summary>
    /// When set, returns only this many testimonials, the most recently added first; when empty, all of them in display order.
    /// </summary>
    public int? Latest { get; init; }
}
