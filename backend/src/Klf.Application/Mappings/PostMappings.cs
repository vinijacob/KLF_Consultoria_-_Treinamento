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
        post.AuthorId,
        post.CategoryId,
        [.. post.Tags.Select(link => link.Tag.Name)]);

    public static PostListItemResponse ToListItem(this Post post) => new(
        post.Id,
        post.Type,
        post.Title,
        post.Slug,
        post.Summary,
        post.CoverId,
        post.Status,
        post.PublishedAt,
        post.Category?.ToRef());

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
        post.SeoDescription,
        post.Category?.ToRef(),
        [.. post.Tags.Select(link => new TaxonomyRef(link.Tag.Name, link.Tag.Slug))]);

    private static TaxonomyRef ToRef(this Category category) => new(category.Name, category.Slug);
}
