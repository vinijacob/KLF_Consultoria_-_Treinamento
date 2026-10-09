using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Klf.Api.Tests.Fakes;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klf.Api.Tests.Controllers;

public sealed class SiteSettingsControllersTests : IClassFixture<KlfApiFactory>
{
    private const string NoPanelRole = "Visitor";
    private static readonly Uri PublicUri = new("/api/v1/public/settings", UriKind.Relative);
    private static readonly Uri AdminContactUri = new("/api/v1/admin/settings/contact", UriKind.Relative);

    private readonly InMemorySiteSettingRepository _store = new();
    private readonly WebApplicationFactory<Program> _factory;

    public SiteSettingsControllersTests(KlfApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISiteSettingRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<ISiteSettingRepository>(_store);
            services.AddSingleton<IUnitOfWork>(_store);
        }));
    }

    [Fact]
    public async Task Public_list_returns_empty_object_when_nothing_was_saved()
    {
        using var client = _factory.CreateClient();

        var json = await client.GetStringAsync(PublicUri, TestContext.Current.CancellationToken);

        Assert.Equal("{}", json);
    }

    [Fact]
    public async Task Admin_put_saves_setting_and_public_list_shows_it()
    {
        using var admin = CreateClientAs(Roles.Admin);
        using var visitor = _factory.CreateClient();

        var put = await admin.PutAsJsonAsync(AdminContactUri, new { whatsapp = "5592999999999", email = "contato@klf.com.br" }, TestContext.Current.CancellationToken);
        var json = await visitor.GetStringAsync(PublicUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        using var document = JsonDocument.Parse(json);
        Assert.Equal("5592999999999", document.RootElement.GetProperty("contact").GetProperty("whatsapp").GetString());
    }

    [Fact]
    public async Task Admin_put_returns_401_when_token_is_missing()
    {
        using var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync(AdminContactUri, new { email = "a@b.com" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_put_returns_403_when_user_has_no_panel_role()
    {
        using var client = CreateClientAs(NoPanelRole);

        var response = await client.PutAsJsonAsync(AdminContactUri, new { email = "a@b.com" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_put_returns_400_in_portuguese_when_value_is_invalid()
    {
        using var client = CreateClientAs(Roles.Admin);

        var response = await client.PutAsJsonAsync(AdminContactUri, new { whatsapp = "abc", mapUrl = "http://x.com" }, TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(["Informe um link válido que comece com https://."], problem.Errors["MapUrl"]);
        Assert.Empty(_store.Settings);
    }

    [Fact]
    public async Task Admin_put_returns_400_when_value_has_unknown_field()
    {
        using var client = CreateClientAs(Roles.Admin);

        var response = await client.PutAsJsonAsync(AdminContactUri, new { whatsap = "5592999999999" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Admin_put_returns_404_when_key_does_not_exist()
    {
        using var client = CreateClientAs(Roles.Admin);

        var response = await client.PutAsJsonAsync(new Uri("/api/v1/admin/settings/inventada", UriKind.Relative), new { a = 1 }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_get_returns_404_when_setting_was_never_saved()
    {
        using var client = CreateClientAs(Roles.Editor);

        var response = await client.GetAsync(AdminContactUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
