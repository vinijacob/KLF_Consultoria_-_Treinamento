using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Posts;

/// <summary>Data to create a post. The author is the signed-in user.</summary>
/// <param name="Type">Kind of post: <c>Project</c>, <c>Article</c> or <c>News</c>.</param>
/// <param name="Title">Headline (up to 200 characters).</param>
/// <param name="Slug">Unique URL identifier: lowercase letters, numbers and hyphens (up to 200 characters).</param>
/// <param name="Summary">Short text for lists (up to 500 characters); optional.</param>
/// <param name="ContentJson">Body in the rich text editor's JSON format.</param>
/// <param name="ContentHtml">Body as HTML; the server removes unsafe markup before saving.</param>
/// <param name="CoverId">Cover image id; optional.</param>
/// <param name="Status">Publishing status: <c>Draft</c>, <c>Scheduled</c> or <c>Published</c>.</param>
/// <param name="ScheduledFor">When to publish (with time zone offset); required when the status is <c>Scheduled</c>.</param>
/// <param name="SeoTitle">Title for search engines (up to 60 characters); optional.</param>
/// <param name="SeoDescription">Description for search engines (up to 160 characters); optional.</param>
public sealed record CreatePostRequest(
    PostType Type,
    string Title,
    string Slug,
    string? Summary,
    string ContentJson,
    string ContentHtml,
    Guid? CoverId,
    PostStatus Status,
    DateTimeOffset? ScheduledFor,
    string? SeoTitle,
    string? SeoDescription) : IPostFields;
