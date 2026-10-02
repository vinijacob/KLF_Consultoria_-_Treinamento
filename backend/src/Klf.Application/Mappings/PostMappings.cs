using Klf.Application.DTOs.Posts;
using Klf.Domain.Entities;

namespace Klf.Application.Mappings;

internal static class PostMappings
{
    public static PostResponse ToResponse(this Post post) => new(
        post.Id,
        post.Type,
        post.Title,
        post.Slug,
        post.Summary,
        post.ContentJson,
        post.ContentHtml,
        post.CoverId,
        post.Status,
        post.PublishedAt,
        post.SeoTitle,
        post.SeoDescription,
        post.AuthorId);

    public static PostListItemResponse ToListItem(this Post post) => new(
        post.Id,
        post.Type,
        post.Title,
        post.Slug,
        post.Summary,
        post.CoverId,
        post.Status,
        post.PublishedAt);

    public static PublicPostResponse ToPublicResponse(this Post post) => new(
        post.Id,
        post.Type,
        post.Title,
        post.Slug,
        post.Summary,
        post.ContentHtml,
        post.CoverId,
        post.PublishedAt,
        post.SeoTitle,
        post.SeoDescription);
}
