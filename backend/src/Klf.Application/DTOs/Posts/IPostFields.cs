using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Posts;

/// <summary>Editable fields shared by the create and update requests, so both use the same validation rules.</summary>
public interface IPostFields
{
    /// <summary>Kind of post.</summary>
    PostType Type { get; }

    /// <summary>Headline.</summary>
    string Title { get; }

    /// <summary>URL-friendly identifier.</summary>
    string Slug { get; }

    /// <summary>Short text for lists.</summary>
    string? Summary { get; }

    /// <summary>Body in the rich text editor's JSON format.</summary>
    string ContentJson { get; }

    /// <summary>Body as HTML (sanitized by the server before saving).</summary>
    string ContentHtml { get; }

    /// <summary>Cover image id.</summary>
    Guid? CoverId { get; }

    /// <summary>Publishing status.</summary>
    PostStatus Status { get; }

    /// <summary>When to publish, required for <see cref="PostStatus.Scheduled"/>.</summary>
    DateTimeOffset? ScheduledFor { get; }

    /// <summary>Title for search engines.</summary>
    string? SeoTitle { get; }

    /// <summary>Description for search engines.</summary>
    string? SeoDescription { get; }
}
