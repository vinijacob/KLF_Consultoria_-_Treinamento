using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Posts;

/// <summary>A post with every field, for the admin panel.</summary>
/// <param name="Id">Post identifier.</param>
/// <param name="Type">Kind of post.</param>
/// <param name="Title">Headline.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="Summary">Short text for lists.</param>
/// <param name="ContentJson">Body in the rich text editor's JSON format.</param>
/// <param name="ContentHtml">Body as sanitized HTML.</param>
/// <param name="CoverId">Cover image id.</param>
/// <param name="Status">Publishing status.</param>
/// <param name="PublishedAt">When it goes (or went) live, in UTC; empty for drafts.</param>
/// <param name="SeoTitle">Title for search engines.</param>
/// <param name="SeoDescription">Description for search engines.</param>
/// <param name="AuthorId">User who created the post.</param>
public sealed record PostResponse(
    Guid Id,
    PostType Type,
    string Title,
    string Slug,
    string? Summary,
    string ContentJson,
    string ContentHtml,
    Guid? CoverId,
    PostStatus Status,
    DateTime? PublishedAt,
    string? SeoTitle,
    string? SeoDescription,
    Guid AuthorId);
