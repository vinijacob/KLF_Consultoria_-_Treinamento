using Klf.Application.DTOs.Posts;
using Klf.Application.Services.Posts;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class PostServiceTests
{
    private static readonly Guid Author = Guid.CreateVersion7();

    private readonly InMemoryPostRepository _repository = new();
    private readonly InMemoryMediaAssetRepository _media = new();
    private readonly MutableTimeProvider _clock = new(new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly PostService _service;

    public PostServiceTests()
    {
        _service = new PostService(_repository, _media, _repository, new FakeHtmlSanitizer(), _clock);
    }

    [Fact]
    public async Task Create_saves_post_with_author_sanitized_html_and_trimmed_text()
    {
        var request = Request("novo-post", PostStatus.Published) with { Title = "  Novo  ", ContentHtml = "<p>ok</p><script>", Summary = "   " };

        var response = await _service.CreateAsync(Author, request, TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Posts);
        Assert.Equal(Author, saved.AuthorId);
        Assert.Equal("Novo", saved.Title);
        Assert.Equal("<p>ok</p>", saved.ContentHtml);
        Assert.Null(saved.Summary);
        Assert.Equal(response.Id, saved.Id);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Create_throws_conflict_when_slug_is_already_used()
    {
        Seed("repetido", PostStatus.Draft);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(Author, Request("repetido", PostStatus.Draft), TestContext.Current.CancellationToken));

        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Update_accepts_own_slug_but_rejects_slug_of_another_post()
    {
        var first = Seed("primeiro", PostStatus.Draft);
        Seed("segundo", PostStatus.Draft);
        var sameSlug = new UpdatePostRequest(PostType.Article, "Novo título", "primeiro", null, "{}", "<p>x</p>", null, PostStatus.Draft, null, null, null);

        await _service.UpdateAsync(first.Id, sameSlug, TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.UpdateAsync(first.Id, sameSlug with { Slug = "segundo" }, TestContext.Current.CancellationToken));

        Assert.Equal("Novo título", first.Title);
    }

    [Fact]
    public async Task Update_throws_not_found_when_post_does_not_exist()
    {
        var request = new UpdatePostRequest(PostType.Article, "T", "t", null, "{}", "<p>x</p>", null, PostStatus.Draft, null, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(Guid.CreateVersion7(), request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Delete_removes_post_when_it_exists()
    {
        var post = Seed("apagar", PostStatus.Published);

        await _service.DeleteAsync(post.Id, TestContext.Current.CancellationToken);

        Assert.Empty(_repository.Posts);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Public_list_hides_drafts_and_future_posts_and_shows_live_ones()
    {
        Seed("rascunho", PostStatus.Draft);
        Seed("futuro", PostStatus.Scheduled, _clock.Now.UtcDateTime.AddDays(1));
        Seed("publicado", PostStatus.Published);

        var page = await _service.ListPublicAsync(new PostListRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(["publicado"], page.Items.Select(p => p.Slug));
        Assert.Equal(1, page.TotalItems);
    }

    [Fact]
    public async Task Public_list_shows_scheduled_post_when_its_date_has_arrived()
    {
        Seed("agendado", PostStatus.Scheduled, _clock.Now.UtcDateTime.AddHours(1));
        _clock.Now = _clock.Now.AddHours(2);

        var page = await _service.ListPublicAsync(new PostListRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(["agendado"], page.Items.Select(p => p.Slug));
    }

    [Fact]
    public async Task Public_list_filters_by_type_and_paginates()
    {
        for (var i = 0; i < 5; i++)
        {
            Seed($"noticia-{i}", PostStatus.Published, type: PostType.News);
        }

        Seed("artigo", PostStatus.Published, type: PostType.Article);

        var page = await _service.ListPublicAsync(new PostListRequest { Type = PostType.News, Page = 2, PageSize = 2 }, TestContext.Current.CancellationToken);

        Assert.Equal(2, page.Items.Count);
        Assert.Equal(5, page.TotalItems);
        Assert.Equal(3, page.TotalPages);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public async Task Public_get_by_slug_throws_not_found_when_post_is_a_draft()
    {
        Seed("secreto", PostStatus.Draft);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetPublicBySlugAsync("secreto", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Public_get_by_slug_returns_post_without_editor_json_when_post_is_published()
    {
        Seed("publico", PostStatus.Published);

        var post = await _service.GetPublicBySlugAsync("publico", TestContext.Current.CancellationToken);

        Assert.Equal("publico", post.Slug);
        Assert.Equal("<p>x</p>", post.ContentHtml);
    }

    [Fact]
    public async Task Admin_list_includes_drafts_and_filters_by_status()
    {
        Seed("rascunho", PostStatus.Draft);
        Seed("publicado", PostStatus.Published);

        var all = await _service.ListAsync(new AdminPostListRequest(), TestContext.Current.CancellationToken);
        var drafts = await _service.ListAsync(new AdminPostListRequest { Status = PostStatus.Draft }, TestContext.Current.CancellationToken);

        Assert.Equal(2, all.TotalItems);
        Assert.Equal(["rascunho"], drafts.Items.Select(p => p.Slug));
    }

    private static CreatePostRequest Request(string slug, PostStatus status) =>
        new(PostType.Article, "Título", slug, null, "{}", "<p>x</p>", null, status, null, null, null);

    private Post Seed(string slug, PostStatus status, DateTime? scheduledFor = null, PostType type = PostType.Article)
    {
        var post = new Post(Author, type, "Título " + slug, slug, null, "{}", "<p>x</p>", null, status, scheduledFor, null, null, _clock.Now.UtcDateTime);
        _repository.Posts.Add(post);
        return post;
    }
}
