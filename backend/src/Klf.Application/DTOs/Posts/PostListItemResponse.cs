using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Posts;

/// <summary>A post in a list: no body, so lists stay small.</summary>
/// <param name="Id">Post identifier.</param>
/// <param name="Type">Kind of post.</param>
/// <param name="Title">Headline.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="Summary">Short text for lists.</param>
/// <param name="CoverId">Cover image id.</param>
/// <param name="Status">Publishing status.</param>
/// <param name="PublishedAt">When it goes (or went) live, in UTC; empty for drafts.</param>
public sealed record PostListItemResponse(
    Guid Id,
    PostType Type,
    string Title,
    string Slug,
    string? Summary,
    Guid? CoverId,
    PostStatus Status,
    DateTime? PublishedAt);
