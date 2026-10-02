using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Tests.Entities;

public sealed class PostTests
{
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Author = Guid.CreateVersion7();

    [Fact]
    public void Published_date_is_now_when_post_is_published_for_the_first_time()
    {
        var post = Create(PostStatus.Published);

        Assert.Equal(Now, post.PublishedAt);
    }

    [Fact]
    public void Published_date_is_kept_when_post_is_updated_while_published()
    {
        var post = Create(PostStatus.Published);

        post.Update(PostType.News, "Novo título", "novo-titulo", null, "{}", "<p>x</p>", null, PostStatus.Published, null, null, null, Now.AddDays(5));

        Assert.Equal(Now, post.PublishedAt);
    }

    [Fact]
    public void Published_date_is_cleared_when_post_goes_back_to_draft()
    {
        var post = Create(PostStatus.Published);

        post.Update(PostType.News, "T", "t", null, "{}", "<p>x</p>", null, PostStatus.Draft, null, null, null, Now);

        Assert.Null(post.PublishedAt);
    }

    [Fact]
    public void Scheduled_date_is_used_when_post_is_scheduled_in_the_future()
    {
        var post = Create(PostStatus.Scheduled, Now.AddDays(2));

        Assert.Equal(Now.AddDays(2), post.PublishedAt);
    }

    [Fact]
    public void Creation_fails_when_scheduled_without_date()
    {
        var exception = Assert.Throws<ValidationException>(() => Create(PostStatus.Scheduled));

        Assert.True(exception.Errors.ContainsKey("ScheduledFor"));
    }

    [Fact]
    public void Creation_fails_when_scheduled_date_is_in_the_past()
    {
        Assert.Throws<ValidationException>(() => Create(PostStatus.Scheduled, Now.AddMinutes(-1)));
    }

    [Fact]
    public void Update_keeps_past_date_when_scheduled_post_already_went_live_and_date_is_unchanged()
    {
        var post = Create(PostStatus.Scheduled, Now.AddDays(1));
        var later = Now.AddDays(2);

        post.Update(PostType.News, "Outro título", "t", null, "{}", "<p>x</p>", null, PostStatus.Scheduled, Now.AddDays(1), null, null, later);

        Assert.Equal("Outro título", post.Title);
    }

    [Fact]
    public void Published_date_becomes_now_when_scheduled_post_is_published_early()
    {
        var post = Create(PostStatus.Scheduled, Now.AddDays(5));

        post.Update(PostType.News, "T", "t", null, "{}", "<p>x</p>", null, PostStatus.Published, null, null, null, Now);

        Assert.Equal(Now, post.PublishedAt);
    }

    [Fact]
    public void Failed_update_keeps_previous_values_when_scheduled_date_is_invalid()
    {
        var post = Create(PostStatus.Draft);

        Assert.Throws<ValidationException>(() =>
            post.Update(PostType.News, "Mudou", "mudou", null, "{}", "<p>x</p>", null, PostStatus.Scheduled, null, null, null, Now));

        Assert.Equal("Título", post.Title);
    }

    [Fact]
    public void Author_is_set_on_creation()
    {
        Assert.Equal(Author, Create(PostStatus.Draft).AuthorId);
    }

    [Fact]
    public void Visibility_rule_hides_drafts_and_future_posts_and_shows_live_ones()
    {
        var isVisible = Post.IsVisible(Now).Compile();

        Assert.False(isVisible(Create(PostStatus.Draft)));
        Assert.False(isVisible(Create(PostStatus.Scheduled, Now.AddHours(1))));
        Assert.True(isVisible(Create(PostStatus.Published)));
        Assert.True(Post.IsVisible(Now.AddHours(2)).Compile()(Create(PostStatus.Scheduled, Now.AddHours(1))));
    }

    private static Post Create(PostStatus status, DateTime? scheduledFor = null) =>
        new(Author, PostType.Article, "Título", "titulo", null, "{}", "<p>x</p>", null, status, scheduledFor, null, null, Now);
}
