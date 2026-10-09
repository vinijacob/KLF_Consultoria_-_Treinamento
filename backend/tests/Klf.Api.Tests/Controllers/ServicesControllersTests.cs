using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Klf.Api.Tests.Fakes;
using Klf.Application.DTOs.Catalog;
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

public sealed class ServicesControllersTests : IClassFixture<KlfApiFactory>
{
    private const string NoPanelRole = "Visitor";
    private static readonly Uri AdminUri = new("/api/v1/admin/services", UriKind.Relative);
    private static readonly Uri PublicUri = new("/api/v1/public/services", UriKind.Relative);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly InMemoryServiceRepository _store = new();
    private readonly WebApplicationFactory<Program> _factory;

    public ServicesControllersTests(KlfApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IServiceRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<IServiceRepository>(_store);
            services.AddSingleton<IUnitOfWork>(_store);
        }));
    }

    [Fact]
    public async Task Public_list_returns_only_active_services_with_format_as_text_when_not_logged_in()
    {
        Seed("ativo", active: true);
        Seed("oculto", active: false);
        using var client = _factory.CreateClient();

        var json = await client.GetStringAsync(PublicUri, TestContext.Current.CancellationToken);

        Assert.Contains("\"slug\":\"ativo\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("oculto", json, StringComparison.Ordinal);
        Assert.Contains("\"format\":\"InCompany\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Public_get_returns_404_when_service_is_inactive_and_200_when_active()
    {
        Seed("oculto", active: false);
        Seed("aberto", active: true);
        using var client = _factory.CreateClient();

        var inactive = await client.GetAsync(new Uri("/api/v1/public/services/oculto", UriKind.Relative), TestContext.Current.CancellationToken);
        var active = await client.GetAsync(new Uri("/api/v1/public/services/aberto", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, inactive.StatusCode);
        Assert.Equal(HttpStatusCode.OK, active.StatusCode);
    }

    [Fact]
    public async Task Admin_list_returns_401_when_token_is_missing()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(AdminUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_create_returns_403_when_user_has_no_panel_role()
    {
        using var client = CreateClientAs(NoPanelRole);

        var response = await client.PostAsJsonAsync(AdminUri, ValidRequest("novo"), JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_create_returns_201_with_location_and_removes_script_when_user_is_editor()
    {
        using var client = CreateClientAs(Roles.Editor);
        var request = ValidRequest("novo") with { ContentHtml = "<p onclick=\"x()\">ok</p><script>alert(1)</script>" };

        var response = await client.PostAsJsonAsync(AdminUri, request, JsonOptions, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<ServiceResponse>(JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith($"/api/v1/admin/services/{body!.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("<p>ok</p>", body.ContentHtml);
    }

    [Fact]
    public async Task Admin_create_returns_409_when_slug_is_already_used()
    {
        Seed("repetido", active: true);
        using var client = CreateClientAs(Roles.Admin);

        var response = await client.PostAsJsonAsync(AdminUri, ValidRequest("repetido"), JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Admin_create_returns_400_in_portuguese_when_request_is_invalid()
    {
        using var client = CreateClientAs(Roles.Admin);
        var request = ValidRequest("Slug Inválido") with { WorkloadHours = 0 };

        var response = await client.PostAsJsonAsync(AdminUri, request, JsonOptions, TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(["O slug deve conter apenas letras minúsculas, números e hífens."], problem.Errors["Slug"]);
        Assert.Equal(["A carga horária deve estar entre 1 e 2000 horas."], problem.Errors["WorkloadHours"]);
    }

    [Fact]
    public async Task Admin_get_update_and_delete_work_when_service_exists()
    {
        var service = Seed("editar", active: true);
        using var client = CreateClientAs(Roles.Admin);
        var uri = new Uri($"{AdminUri}/{service.Id}", UriKind.Relative);

        var get = await client.GetAsync(uri, TestContext.Current.CancellationToken);
        var put = await client.PutAsJsonAsync(uri, ValidRequest("editar") with { Title = "Editado" }, JsonOptions, TestContext.Current.CancellationToken);
        var delete = await client.DeleteAsync(uri, TestContext.Current.CancellationToken);
        var getAfter = await client.GetAsync(uri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
    }

    private static CreateServiceRequest ValidRequest(string slug) =>
        new("Título", slug, null, "<p>x</p>", "Gestores", 8, ServiceFormat.InCompany, null, 0, true);

    private Service Seed(string slug, bool active)
    {
        var service = new Service("Título " + slug, slug, null, "<p>x</p>", "Gestores", 8, ServiceFormat.InCompany, null, 0, active);
        _store.Services.Add(service);
        return service;
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
