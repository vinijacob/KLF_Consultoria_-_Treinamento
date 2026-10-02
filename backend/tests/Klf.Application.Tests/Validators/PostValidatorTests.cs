using Klf.Application.DTOs.Posts;
using Klf.Application.Validators.Posts;
using Klf.Domain.Enums;

namespace Klf.Application.Tests.Validators;

public sealed class PostValidatorTests
{
    private readonly CreatePostRequestValidator _validator = new();

    [Fact]
    public void Request_is_valid_when_only_required_fields_are_filled()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Theory]
    [InlineData("Com Espaço")]
    [InlineData("MAIUSCULA")]
    [InlineData("-comeca-com-hifen")]
    [InlineData("termina-com-hifen-")]
    [InlineData("duplo--hifen")]
    [InlineData("acentuação")]
    public void Slug_is_rejected_when_it_is_not_url_friendly(string slug)
    {
        var result = _validator.Validate(Valid() with { Slug = slug });

        Assert.Contains(result.Errors, e => e.PropertyName == "Slug");
    }

    [Fact]
    public void Content_is_rejected_when_json_is_malformed()
    {
        var result = _validator.Validate(Valid() with { ContentJson = "{isso nao e json" });

        Assert.Contains(result.Errors, e => e.PropertyName == "ContentJson");
    }

    [Fact]
    public void Schedule_date_is_required_when_status_is_scheduled()
    {
        var result = _validator.Validate(Valid() with { Status = PostStatus.Scheduled, ScheduledFor = null });

        Assert.Contains(result.Errors, e => e.PropertyName == "ScheduledFor");
    }

    [Fact]
    public void Every_problem_is_reported_when_request_has_several_errors()
    {
        var request = new CreatePostRequest((PostType)99, "", "", new string('x', 501), "", "", Guid.Empty, (PostStatus)99, null, new string('x', 61), new string('x', 161));

        var fields = _validator.Validate(request).Errors.Select(e => e.PropertyName).ToHashSet();

        Assert.Equal(
            ["ContentHtml", "ContentJson", "CoverId", "SeoDescription", "SeoTitle", "Slug", "Status", "Summary", "Title", "Type"],
            fields.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void List_request_is_invalid_when_page_is_zero_or_search_is_too_long()
    {
        var validator = new PostListRequestValidator();

        var result = validator.Validate(new PostListRequest { Page = 0, Search = new string('x', 101) });

        Assert.Equal(["Page", "Search"], result.Errors.Select(e => e.PropertyName).Order(StringComparer.Ordinal));
    }

    private static CreatePostRequest Valid() =>
        new(PostType.Article, "Título", "titulo-do-post", null, "{\"type\":\"doc\"}", "<p>x</p>", null, PostStatus.Draft, null, null, null);
}
