using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Posts;

/// <summary>A published post as the public site needs it. It has no editor JSON, status or author id.</summary>
/// <param name="Id">Post identifier.</param>
/// <param name="Type">Kind of post.</param>
/// <param name="Title">Headline.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="Summary">Short text for lists and share previews.</param>
/// <param name="ContentHtml">Body as sanitized HTML.</param>
/// <param name="CoverId">Cover image id.</param>
/// <param name="PublishedAt">When it went live, in UTC.</param>
/// <param name="SeoTitle">Title for search engines; empty means use <paramref name="Title"/>.</param>
/// <param name="SeoDescription">Description for search engines; empty means use <paramref name="Summary"/>.</param>
public sealed record PublicPostResponse(
    Guid Id,
    PostType Type,
    string Title,
    string Slug,
    string? Summary,
    string ContentHtml,
    Guid? CoverId,
    DateTime? PublishedAt,
    string? SeoTitle,
    string? SeoDescription);
