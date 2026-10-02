using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Klf.Domain.Common;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Entities;

/// <summary>A blog post, project or news item published on the site.</summary>
public sealed class Post : SoftDeletableEntity
{
    private const string ScheduledForField = "ScheduledFor";

    /// <summary>Creates a post.</summary>
    /// <param name="authorId">Id of the signed-in user who creates it (never taken from the request body).</param>
    /// <param name="type">Kind of post.</param>
    /// <param name="title">Headline.</param>
    /// <param name="slug">Unique URL identifier.</param>
    /// <param name="summary">Short text for lists; optional.</param>
    /// <param name="contentJson">Body in the rich text editor's JSON format.</param>
    /// <param name="contentHtml">Body as sanitized HTML.</param>
    /// <param name="coverId">Cover image id; optional.</param>
    /// <param name="status">Publishing status.</param>
    /// <param name="scheduledFor">When it should go live (UTC); required when <paramref name="status"/> is <see cref="PostStatus.Scheduled"/>.</param>
    /// <param name="seoTitle">Title for search engines; optional.</param>
    /// <param name="seoDescription">Description for search engines; optional.</param>
    /// <param name="utcNow">Current time, in UTC.</param>
    /// <exception cref="ValidationException">The status is <see cref="PostStatus.Scheduled"/> and the date is missing or not in the future.</exception>
    public Post(
        Guid authorId,
        PostType type,
        string title,
        string slug,
        string? summary,
        string contentJson,
        string contentHtml,
        Guid? coverId,
        PostStatus status,
        DateTime? scheduledFor,
        string? seoTitle,
        string? seoDescription,
        DateTime utcNow)
    {
        AuthorId = authorId;

        SetDetails(type, title, slug, summary, contentJson, contentHtml, coverId, status, scheduledFor, seoTitle, seoDescription, utcNow);
    }

    // Used only by EF Core to materialize rows: the public constructor has parameters (scheduledFor, utcNow) that are not properties.
    private Post()
    {
        Title = string.Empty;
        Slug = string.Empty;
        ContentJson = string.Empty;
        ContentHtml = string.Empty;
    }

    /// <summary>Kind of post.</summary>
    public PostType Type { get; private set; }

    /// <summary>Headline.</summary>
    public string Title { get; private set; }

    /// <summary>Unique, URL-friendly identifier (e.g. <c>como-motivar-equipes</c>), used in <c>/blog/{slug}</c>.</summary>
    public string Slug { get; private set; }

    /// <summary>Short text shown in lists and share previews; optional.</summary>
    public string? Summary { get; private set; }

    /// <summary>Body in the rich text editor's JSON format; used to edit the post again.</summary>
    public string ContentJson { get; private set; }

    /// <summary>Body as sanitized HTML; this is what the public site renders.</summary>
    public string ContentHtml { get; private set; }

    /// <summary>Cover image (will reference a <c>MediaAsset</c> once the images module exists); optional.</summary>
    public Guid? CoverId { get; private set; }

    /// <summary>Publishing status.</summary>
    public PostStatus Status { get; private set; }

    /// <summary>
    /// When the post goes (or went) live, in UTC: the schedule date for <see cref="PostStatus.Scheduled"/>,
    /// the publication moment for <see cref="PostStatus.Published"/>, and <see langword="null"/> for drafts.
    /// </summary>
    public DateTime? PublishedAt { get; private set; }

    /// <summary>Title for search engines and share previews (up to 60 characters); the site falls back to <see cref="Title"/>.</summary>
    public string? SeoTitle { get; private set; }

    /// <summary>Description for search engines (up to 160 characters); the site falls back to <see cref="Summary"/>.</summary>
    public string? SeoDescription { get; private set; }

    /// <summary>User who created the post. Never changes after creation.</summary>
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// The rule for "visible on the public site": not a draft and the publication date already arrived.
    /// Written as an expression so repositories can use it in database queries.
    /// </summary>
    public static Expression<Func<Post, bool>> IsVisible(DateTime utcNow) =>
        post => post.Status != PostStatus.Draft && post.PublishedAt <= utcNow;

    /// <summary>Replaces every editable field. The author is kept.</summary>
    /// <param name="type">Kind of post.</param>
    /// <param name="title">Headline.</param>
    /// <param name="slug">Unique URL identifier.</param>
    /// <param name="summary">Short text for lists; optional.</param>
    /// <param name="contentJson">Body in the rich text editor's JSON format.</param>
    /// <param name="contentHtml">Body as sanitized HTML.</param>
    /// <param name="coverId">Cover image id; optional.</param>
    /// <param name="status">Publishing status.</param>
    /// <param name="scheduledFor">When it should go live (UTC); required when <paramref name="status"/> is <see cref="PostStatus.Scheduled"/>.</param>
    /// <param name="seoTitle">Title for search engines; optional.</param>
    /// <param name="seoDescription">Description for search engines; optional.</param>
    /// <param name="utcNow">Current time, in UTC.</param>
    /// <exception cref="ValidationException">The status is <see cref="PostStatus.Scheduled"/> and the date is missing or not in the future.</exception>
    public void Update(
        PostType type,
        string title,
        string slug,
        string? summary,
        string contentJson,
        string contentHtml,
        Guid? coverId,
        PostStatus status,
        DateTime? scheduledFor,
        string? seoTitle,
        string? seoDescription,
        DateTime utcNow)
    {
        SetDetails(type, title, slug, summary, contentJson, contentHtml, coverId, status, scheduledFor, seoTitle, seoDescription, utcNow);
    }

    [MemberNotNull(nameof(Title), nameof(Slug), nameof(ContentJson), nameof(ContentHtml))]
    private void SetDetails(
        PostType type,
        string title,
        string slug,
        string? summary,
        string contentJson,
        string contentHtml,
        Guid? coverId,
        PostStatus status,
        DateTime? scheduledFor,
        string? seoTitle,
        string? seoDescription,
        DateTime utcNow)
    {
        var publishedAt = ResolvePublishedAt(status, scheduledFor, utcNow);

        Type = type;
        Title = title;
        Slug = slug;
        Summary = summary;
        ContentJson = contentJson;
        ContentHtml = contentHtml;
        CoverId = coverId;
        Status = status;
        PublishedAt = publishedAt;
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
    }

    private DateTime? ResolvePublishedAt(PostStatus status, DateTime? scheduledFor, DateTime utcNow)
    {
        switch (status)
        {
            case PostStatus.Published:
                return PublishedAt is { } alreadyLive && alreadyLive <= utcNow ? alreadyLive : utcNow;

            case PostStatus.Scheduled:
                if (scheduledFor is null)
                {
                    throw new ValidationException(ScheduledForField, "Informe a data do agendamento.");
                }

                if (scheduledFor <= utcNow && scheduledFor != PublishedAt)
                {
                    throw new ValidationException(ScheduledForField, "A data do agendamento precisa ser no futuro.");
                }

                return scheduledFor;

            default:
                return null;
        }
    }
}
