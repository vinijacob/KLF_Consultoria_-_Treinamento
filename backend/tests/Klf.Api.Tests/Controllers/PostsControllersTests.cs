using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Klf.Api.Tests.Fakes;
using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Posts;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Enums;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klf.Api.Tests.Controllers;

public sealed class PostsControllersTests : IClassFixture<KlfApiFactory>
{
    private static readonly Uri AdminUri = new("/api/v1/admin/posts", UriKind.Relative);
    private static readonly Uri PublicUri = new("/api/v1/public/posts", UriKind.Relative);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly InMemoryPostRepository _store = new();
    private readonly WebApplicationFactory<Program> _factory;

    public PostsControllersTests(KlfApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPostRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<IPostRepository>(_store);
            services.AddSingleton<IUnitOfWork>(_store);
        }));
    }

    [Fact]
    public async Task Public_list_returns_only_published_posts_when_not_logged_in()
    {
        Seed("publicado", PostStatus.Published);
        Seed("rascunho", PostStatus.Draft);
        using var client = _factory.CreateClient();

        var page = await client.GetFromJsonAsync<PagedResponse<PostListItemResponse>>(PublicUri, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(["publicado"], page!.Items.Select(p => p.Slug));
    }

    [Fact]
    public async Task Public_list_returns_400_when_page_is_invalid()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v1/public/posts?page=0", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Public_get_returns_404_when_post_is_a_draft_and_200_when_published()
    {
        Seed("secreto", PostStatus.Draft);
        Seed("aberto", PostStatus.Published);
        using var client = _factory.CreateClient();

        var draft = await client.GetAsync(new Uri("/api/v1/public/posts/secreto", UriKind.Relative), TestContext.Current.CancellationToken);
        var published = await client.GetAsync(new Uri("/api/v1/public/posts/aberto", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, draft.StatusCode);
        Assert.Equal(HttpStatusCode.OK, published.StatusCode);
    }

    [Fact]
    public async Task Admin_list_returns_401_when_token_is_missing()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(AdminUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_create_returns_403_when_user_is_instructor()
    {
        using var client = CreateClientAs(Guid.CreateVersion7(), Roles.Instructor);

        var response = await client.PostAsJsonAsync(AdminUri, ValidRequest("novo"), JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_create_uses_signed_in_user_as_author_and_returns_201()
    {
        var userId = Guid.CreateVersion7();
        using var client = CreateClientAs(userId, Roles.Editor);

        var response = await client.PostAsJsonAsync(AdminUri, ValidRequest("novo-post"), JsonOptions, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<PostResponse>(JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith($"/api/v1/admin/posts/{body!.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(userId, body.AuthorId);
    }

    [Fact]
    public async Task Admin_create_removes_script_from_html()
    {
        using var client = CreateClientAs(Guid.CreateVersion7(), Roles.Admin);
        var request = ValidRequest("xss") with { ContentHtml = "<p onclick=\"alert(1)\">oi</p><script>alert(2)</script>" };

        var response = await client.PostAsJsonAsync(AdminUri, request, JsonOptions, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<PostResponse>(JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal("<p>oi</p>", body!.ContentHtml);
    }

    [Fact]
    public async Task Admin_create_returns_409_when_slug_is_already_used()
    {
        Seed("repetido", PostStatus.Draft);
        using var client = CreateClientAs(Guid.CreateVersion7(), Roles.Admin);

        var response = await client.PostAsJsonAsync(AdminUri, ValidRequest("repetido"), JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Admin_create_returns_400_in_portuguese_when_request_is_invalid()
    {
        using var client = CreateClientAs(Guid.CreateVersion7(), Roles.Admin);
        var request = ValidRequest("Slug Inválido") with { Status = PostStatus.Scheduled };

        var response = await client.PostAsJsonAsync(AdminUri, request, JsonOptions, TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(["O slug deve conter apenas letras minúsculas, números e hífens."], problem.Errors["Slug"]);
        Assert.Equal(["Informe a data do agendamento."], problem.Errors["ScheduledFor"]);
    }

    [Fact]
    public async Task Admin_get_update_and_delete_work_when_post_exists()
    {
        var post = Seed("editar", PostStatus.Draft);
        using var client = CreateClientAs(Guid.CreateVersion7(), Roles.Admin);
        var uri = new Uri($"{AdminUri}/{post.Id}", UriKind.Relative);

        var get = await client.GetAsync(uri, TestContext.Current.CancellationToken);
        var put = await client.PutAsJsonAsync(uri, ValidRequest("editar") with { Title = "Editado" }, JsonOptions, TestContext.Current.CancellationToken);
        var delete = await client.DeleteAsync(uri, TestContext.Current.CancellationToken);
        var getAfter = await client.GetAsync(uri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
    }

    private static CreatePostRequest ValidRequest(string slug) =>
        new(PostType.Article, "Título", slug, null, "{\"type\":\"doc\"}", "<p>x</p>", null, PostStatus.Draft, null, null, null);

    private Post Seed(string slug, PostStatus status)
    {
        var now = DateTime.UtcNow;
        var post = new Post(Guid.CreateVersion7(), PostType.Article, "Título " + slug, slug, null, "{}", "<p>x</p>", null, status, null, null, null, now);
        _store.Posts.Add(post);
        return post;
    }

    private HttpClient CreateClientAs(Guid userId, string role)
    {
        var token = _factory.Services.GetRequiredService<ITokenService>()
            .Generate(new UserAccount(userId, $"{role}@klf.test", role, [role]));
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }
}
