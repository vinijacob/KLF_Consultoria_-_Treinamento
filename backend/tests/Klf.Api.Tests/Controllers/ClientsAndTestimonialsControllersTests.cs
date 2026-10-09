using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Klf.Api.Tests.Fakes;
using Klf.Application.DTOs.Clients;
using Klf.Application.DTOs.Testimonials;
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

public sealed class ClientsAndTestimonialsControllersTests : IClassFixture<KlfApiFactory>
{
    private const string NoPanelRole = "Visitor";
    private static readonly Uri AdminClients = new("/api/v1/admin/clients", UriKind.Relative);
    private static readonly Uri PublicClients = new("/api/v1/public/clients", UriKind.Relative);
    private static readonly Uri AdminTestimonials = new("/api/v1/admin/testimonials", UriKind.Relative);
    private static readonly Uri PublicTestimonials = new("/api/v1/public/testimonials", UriKind.Relative);

    private readonly InMemoryClientRepository _clients = new();
    private readonly InMemoryTestimonialRepository _testimonials = new();
    private readonly WebApplicationFactory<Program> _factory;

    public ClientsAndTestimonialsControllersTests(KlfApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IClientRepository>();
            services.RemoveAll<ITestimonialRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<IClientRepository>(_clients);
            services.AddSingleton<ITestimonialRepository>(_testimonials);
            services.AddSingleton<IUnitOfWork>(_clients);
        }));
    }

    [Fact]
    public async Task Public_clients_list_returns_only_active_clients_when_not_logged_in()
    {
        _clients.Clients.Add(new Client("Loja Ativa", null, null, 0, true));
        _clients.Clients.Add(new Client("Loja Oculta", null, null, 1, false));
        using var client = _factory.CreateClient();

        var json = await client.GetStringAsync(PublicClients, TestContext.Current.CancellationToken);

        Assert.Contains("Loja Ativa", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Oculta", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Admin_clients_return_401_without_token_and_403_without_panel_role()
    {
        using var anonymous = _factory.CreateClient();
        using var outsider = CreateClientAs(NoPanelRole);

        var unauthorized = await anonymous.GetAsync(AdminClients, TestContext.Current.CancellationToken);
        var forbidden = await outsider.GetAsync(AdminClients, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Admin_clients_create_update_and_delete_work_when_user_is_editor()
    {
        using var client = CreateClientAs(Roles.Editor);

        var create = await client.PostAsJsonAsync(AdminClients, new CreateClientRequest("Loja", "https://loja.com.br", null, 0, true), TestContext.Current.CancellationToken);
        var body = await create.Content.ReadFromJsonAsync<ClientResponse>(TestContext.Current.CancellationToken);
        var uri = new Uri($"{AdminClients}/{body!.Id}", UriKind.Relative);
        var put = await client.PutAsJsonAsync(uri, new UpdateClientRequest("Loja Nova", null, null, 1, true), TestContext.Current.CancellationToken);
        var delete = await client.DeleteAsync(uri, TestContext.Current.CancellationToken);
        var after = await client.GetAsync(uri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.EndsWith($"/api/v1/admin/clients/{body.Id}", create.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);
    }

    [Fact]
    public async Task Admin_clients_create_returns_400_in_portuguese_when_link_is_not_https()
    {
        using var client = CreateClientAs(Roles.Admin);

        var response = await client.PostAsJsonAsync(AdminClients, new CreateClientRequest("Loja", "http://x.com", null, 0, true), TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["O link deve ser um endereço válido que comece com https://."], problem!.Errors["WebsiteUrl"]);
    }

    [Fact]
    public async Task Public_testimonials_hide_drafts_revoked_and_consent_data()
    {
        var signed = DateTime.UtcNow.AddDays(-5);
        _testimonials.Testimonials.Add(new Testimonial("Ana Visível", "Gerente", null, "Ótimo", null, signed, false, true, 0));
        _testimonials.Testimonials.Add(new Testimonial("Beto Rascunho", null, null, "x", null, signed, false, false, 1));
        var revoked = new Testimonial("Caio Revogado", null, null, "x", null, signed, false, true, 2);
        revoked.RevokeConsent(DateTime.UtcNow);
        _testimonials.Testimonials.Add(revoked);
        using var client = _factory.CreateClient();

        var json = await client.GetStringAsync(PublicTestimonials, TestContext.Current.CancellationToken);

        Assert.Contains("Ana Visível", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Beto", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Caio", json, StringComparison.Ordinal);
        Assert.DoesNotContain("consent", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Admin_testimonials_create_returns_400_when_publishing_without_consent()
    {
        using var client = CreateClientAs(Roles.Admin);
        var request = new CreateTestimonialRequest("Ana", null, null, "Ótimo", null, null, false, true, 0);

        var response = await client.PostAsJsonAsync(AdminTestimonials, request, TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ConsentGivenAt", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Admin_testimonials_create_returns_400_when_consent_is_in_the_future()
    {
        using var client = CreateClientAs(Roles.Admin);
        var request = new CreateTestimonialRequest("Ana", null, null, "Ótimo", null, DateTimeOffset.UtcNow.AddDays(3), false, true, 0);

        var response = await client.PostAsJsonAsync(AdminTestimonials, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Revoke_consent_hides_testimonial_from_public_and_blocks_new_publication()
    {
        using var client = CreateClientAs(Roles.Admin);
        var create = await client.PostAsJsonAsync(
            AdminTestimonials,
            new CreateTestimonialRequest("Ana Pública", null, null, "Ótimo", null, DateTimeOffset.UtcNow.AddDays(-1), false, true, 0),
            TestContext.Current.CancellationToken);
        var body = await create.Content.ReadFromJsonAsync<TestimonialResponse>(TestContext.Current.CancellationToken);
        var before = await client.GetStringAsync(PublicTestimonials, TestContext.Current.CancellationToken);

        var revoke = await client.PostAsync(new Uri($"{AdminTestimonials}/{body!.Id}/revoke-consent", UriKind.Relative), null, TestContext.Current.CancellationToken);
        var revoked = await revoke.Content.ReadFromJsonAsync<TestimonialResponse>(TestContext.Current.CancellationToken);
        var after = await client.GetStringAsync(PublicTestimonials, TestContext.Current.CancellationToken);
        var republish = await client.PutAsJsonAsync(
            new Uri($"{AdminTestimonials}/{body.Id}", UriKind.Relative),
            new UpdateTestimonialRequest("Ana Pública", null, null, "Ótimo", null, DateTimeOffset.UtcNow.AddDays(-1), false, true, 0),
            TestContext.Current.CancellationToken);

        Assert.Contains("Ana Pública", before, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, revoke.StatusCode);
        Assert.NotNull(revoked!.ConsentRevokedAt);
        Assert.DoesNotContain("Ana Pública", after, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.BadRequest, republish.StatusCode);
    }

    [Fact]
    public async Task Admin_testimonials_return_403_without_panel_role_and_404_for_unknown_id()
    {
        using var outsider = CreateClientAs(NoPanelRole);
        using var admin = CreateClientAs(Roles.Admin);

        var forbidden = await outsider.GetAsync(AdminTestimonials, TestContext.Current.CancellationToken);
        var notFound = await admin.PostAsync(new Uri($"{AdminTestimonials}/{Guid.CreateVersion7()}/revoke-consent", UriKind.Relative), null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
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
