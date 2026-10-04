using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Klf.Api.Tests.Fakes;
using Klf.Application.DTOs.Albums;
using Klf.Application.DTOs.Clients;
using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Media;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;
using Klf.Domain.Entities;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klf.Api.Tests.Controllers;

public sealed class MediaAndAlbumsControllersTests : IClassFixture<KlfApiFactory>
{
    private static readonly Uri AdminMedia = new("/api/v1/admin/media", UriKind.Relative);
    private static readonly Uri AdminAlbums = new("/api/v1/admin/albums", UriKind.Relative);
    private static readonly Uri PublicAlbums = new("/api/v1/public/albums", UriKind.Relative);
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    private readonly KlfApiFactory _root;
    private readonly InMemoryAlbumRepository _albums = new();
    private readonly InMemoryClientRepository _clients = new();
    private readonly WebApplicationFactory<Program> _factory;

    public MediaAndAlbumsControllersTests(KlfApiFactory factory)
    {
        _root = factory;
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAlbumRepository>();
            services.RemoveAll<IClientRepository>();
            services.AddSingleton<IAlbumRepository>(_albums);
            services.AddSingleton<IClientRepository>(_clients);
        }));
    }

    [Fact]
    public async Task Upload_returns_401_without_token_and_403_for_instructor()
    {
        using var anonymous = _factory.CreateClient();
        using var instructor = CreateClientAs(Roles.Instructor);

        var unauthorized = await anonymous.PostAsync(AdminMedia, Form(Png, "a.png"), TestContext.Current.CancellationToken);
        var forbidden = await instructor.PostAsync(AdminMedia, Form(Png, "a.png"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Upload_returns_201_with_url_and_stores_file_when_image_is_valid()
    {
        using var client = CreateClientAs(Roles.Editor);

        var response = await client.PostAsync(AdminMedia, Form(Png, "foto.png", "Equipe"), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<MediaAssetResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith($"/api/v1/admin/media/{body!.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("https://files.test/media/", body.Url, StringComparison.Ordinal);
        Assert.Equal("image/png", body.ContentType);
        Assert.Equal("Equipe", body.AltText);
        Assert.Contains(_root.Storage.Files, f => body.Url.EndsWith(f.Key, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Upload_returns_400_in_portuguese_when_file_is_not_an_image_or_missing()
    {
        using var client = CreateClientAs(Roles.Admin);

        var notImage = await client.PostAsync(AdminMedia, Form("<script>alert(1)</script>"u8.ToArray(), "x.png"), TestContext.Current.CancellationToken);
        var problem = await notImage.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        var missing = await client.PostAsync(AdminMedia, new MultipartFormDataContent(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, notImage.StatusCode);
        Assert.Equal(["Formato não suportado. Envie uma imagem JPEG, PNG ou WebP."], problem!.Errors["File"]);
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
    }

    [Fact]
    public async Task Media_list_update_and_delete_work_and_delete_returns_409_when_in_use()
    {
        var media = _root.Media.Seed("antigo");
        var used = _root.Media.Seed();
        _root.Media.Referenced.Add(used.Id);
        using var client = CreateClientAs(Roles.Admin);

        var list = await client.GetFromJsonAsync<PagedResponse<MediaAssetResponse>>(new Uri($"{AdminMedia}?pageSize=100", UriKind.Relative), TestContext.Current.CancellationToken);
        var put = await client.PutAsJsonAsync(new Uri($"{AdminMedia}/{media.Id}", UriKind.Relative), new UpdateMediaRequest("novo"), TestContext.Current.CancellationToken);
        var conflict = await client.DeleteAsync(new Uri($"{AdminMedia}/{used.Id}", UriKind.Relative), TestContext.Current.CancellationToken);
        var delete = await client.DeleteAsync(new Uri($"{AdminMedia}/{media.Id}", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Contains(list!.Items, i => i.Id == media.Id);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task Public_media_redirects_to_file_url_and_returns_404_when_unknown()
    {
        var media = _root.Media.Seed();
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var found = await client.GetAsync(new Uri($"/api/v1/public/media/{media.Id}", UriKind.Relative), TestContext.Current.CancellationToken);
        var missing = await client.GetAsync(new Uri($"/api/v1/public/media/{Guid.CreateVersion7()}", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, found.StatusCode);
        Assert.Equal($"https://files.test/{media.StorageKey}", found.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Album_flow_creates_sets_items_and_publishes_to_public_site()
    {
        var first = _root.Media.Seed("primeira");
        var second = _root.Media.Seed("segunda");
        using var admin = CreateClientAs(Roles.Editor);
        using var anonymous = _factory.CreateClient();

        var create = await admin.PostAsJsonAsync(AdminAlbums, new CreateAlbumRequest("Turma Maio", "turma-maio", "Fotos", first.Id, 0, true), TestContext.Current.CancellationToken);
        var album = await create.Content.ReadFromJsonAsync<AlbumResponse>(TestContext.Current.CancellationToken);
        var items = await admin.PutAsJsonAsync(
            new Uri($"{AdminAlbums}/{album!.Id}/items", UriKind.Relative),
            new SetAlbumItemsRequest([new AlbumItemRequest(second.Id, "A"), new AlbumItemRequest(first.Id, null)]),
            TestContext.Current.CancellationToken);
        var publicAlbum = await anonymous.GetFromJsonAsync<PublicAlbumResponse>(new Uri($"{PublicAlbums}/turma-maio", UriKind.Relative), TestContext.Current.CancellationToken);
        var list = await anonymous.GetStringAsync(PublicAlbums, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.Equal(HttpStatusCode.OK, items.StatusCode);
        Assert.Equal(["A", null], publicAlbum!.Items.Select(i => i.Caption));
        Assert.Contains("\"itemCount\":2", list, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Album_create_returns_409_for_used_slug_400_for_missing_cover_and_404_for_unknown_id()
    {
        _albums.Albums.Add(new Album("A", "repetido", null, null, 0, true));
        using var admin = CreateClientAs(Roles.Admin);

        var conflict = await admin.PostAsJsonAsync(AdminAlbums, new CreateAlbumRequest("B", "repetido", null, null, 0, true), TestContext.Current.CancellationToken);
        var badCover = await admin.PostAsJsonAsync(AdminAlbums, new CreateAlbumRequest("B", "novo", null, Guid.CreateVersion7(), 0, true), TestContext.Current.CancellationToken);
        var notFound = await admin.GetAsync(new Uri($"{AdminAlbums}/{Guid.CreateVersion7()}", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, badCover.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Public_album_returns_404_when_inactive()
    {
        _albums.Albums.Add(new Album("Oculto", "oculto", null, null, 0, false));
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(new Uri($"{PublicAlbums}/oculto", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Client_logo_returns_400_when_image_does_not_exist_and_201_when_it_does()
    {
        var logo = _root.Media.Seed();
        using var admin = CreateClientAs(Roles.Admin);
        var uri = new Uri("/api/v1/admin/clients", UriKind.Relative);

        var missing = await admin.PostAsJsonAsync(uri, new CreateClientRequest("Loja", null, Guid.CreateVersion7(), 0, true), TestContext.Current.CancellationToken);
        var ok = await admin.PostAsJsonAsync(uri, new CreateClientRequest("Loja", null, logo.Id, 0, true), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);
    }

    private static MultipartFormDataContent Form(byte[] bytes, string fileName, string? altText = null)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var form = new MultipartFormDataContent { { file, "file", fileName } };

        if (altText is not null)
        {
            form.Add(new StringContent(altText), "altText");
        }

        return form;
    }

    private HttpClient CreateClientAs(string role)
    {
        var token = _factory.Services.GetRequiredService<ITokenService>()
            .Generate(new UserAccount(Guid.CreateVersion7(), $"{role}@klf.test", role, [role]));
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }
}
