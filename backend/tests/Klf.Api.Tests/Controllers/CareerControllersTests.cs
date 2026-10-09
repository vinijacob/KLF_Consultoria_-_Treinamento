using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Klf.Application.DTOs.Career;
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

public sealed class CareerControllersTests : IClassFixture<KlfApiFactory>
{
    private const string NoPanelRole = "Visitor";
    private static readonly Uri AdminUri = new("/api/v1/admin/career", UriKind.Relative);
    private static readonly Uri PublicUri = new("/api/v1/public/career", UriKind.Relative);

    private readonly InMemoryStore _store = new();
    private readonly WebApplicationFactory<Program> _factory;

    public CareerControllersTests(KlfApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICareerEntryRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<ICareerEntryRepository>(_store);
            services.AddSingleton<IUnitOfWork>(_store);
        }));
    }

    [Fact]
    public async Task Public_list_returns_entries_with_type_as_text_when_not_logged_in()
    {
        _store.Entries.Add(new CareerEntry(CareerEntryType.Education, "MBA", "FGV", null, new(2020, 1, 1), null, 0));
        using var client = _factory.CreateClient();

        var json = await client.GetStringAsync(PublicUri, TestContext.Current.CancellationToken);

        Assert.Contains("\"entryType\":\"Education\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Admin_create_returns_401_when_token_is_missing()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(AdminUri, ValidRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_create_returns_403_when_user_has_no_panel_role()
    {
        using var client = CreateClientAs(NoPanelRole);

        var response = await client.PostAsJsonAsync(AdminUri, ValidRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_create_returns_201_with_location_when_user_is_editor()
    {
        using var client = CreateClientAs(Roles.Editor);

        var response = await client.PostAsJsonAsync(AdminUri, ValidRequest(), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<CareerEntryResponse>(JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith($"/api/v1/admin/career/{body!.Id}", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Single(_store.Entries);
    }

    [Fact]
    public async Task Admin_create_returns_400_in_portuguese_when_end_date_is_before_start()
    {
        using var client = CreateClientAs(Roles.Admin);
        var request = ValidRequest() with { EndDate = new DateOnly(2019, 1, 1) };

        var response = await client.PostAsJsonAsync(AdminUri, request, JsonOptions, TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(["A data de término não pode ser anterior à data de início."], problem.Errors["EndDate"]);
    }

    [Fact]
    public async Task Admin_update_returns_404_when_entry_does_not_exist()
    {
        using var client = CreateClientAs(Roles.Admin);
        var request = new UpdateCareerEntryRequest(CareerEntryType.Education, "MBA", null, null, new(2020, 1, 1), null, 0);

        var response = await client.PutAsJsonAsync(new Uri($"{AdminUri}/{Guid.CreateVersion7()}", UriKind.Relative), request, JsonOptions, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_delete_returns_204_and_removes_entry_when_it_exists()
    {
        var entry = new CareerEntry(CareerEntryType.Education, "MBA", "FGV", null, new(2020, 1, 1), null, 0);
        _store.Entries.Add(entry);
        using var client = CreateClientAs(Roles.Admin);

        var response = await client.DeleteAsync(new Uri($"{AdminUri}/{entry.Id}", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(_store.Entries);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    private static CreateCareerEntryRequest ValidRequest() =>
        new(CareerEntryType.Education, "MBA em Gestão de Pessoas", "FGV", null, new(2020, 1, 1), new(2021, 6, 30), 0);

    private HttpClient CreateClientAs(string role)
    {
        var token = _factory.Services.GetRequiredService<ITokenService>()
            .Generate(new UserAccount(Guid.CreateVersion7(), $"{role}@klf.test", role, [role]));
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }

    private sealed class InMemoryStore : ICareerEntryRepository, IUnitOfWork
    {
        public List<CareerEntry> Entries { get; } = [];

        public Task<IReadOnlyList<CareerEntry>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CareerEntry>>([.. Entries]);

        public Task<CareerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Entries.SingleOrDefault(e => e.Id == id));

        public void Add(CareerEntry entry) => Entries.Add(entry);

        public void Remove(CareerEntry entry) => Entries.Remove(entry);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
