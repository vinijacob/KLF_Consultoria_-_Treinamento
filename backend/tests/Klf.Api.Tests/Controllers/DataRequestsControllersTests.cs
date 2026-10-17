using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Klf.Api.Tests.Fakes;
using Klf.Application.DTOs.DataRequests;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Interfaces.Storage;
using Klf.Domain.Common;
using Klf.Domain.Entities;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klf.Api.Tests.Controllers;

public sealed class DataRequestsControllersTests : IClassFixture<KlfApiFactory>
{
    private static readonly Uri PublicUri = new("/api/v1/public/data-requests", UriKind.Relative);
    private static readonly Uri AdminUri = new("/api/v1/admin/data-requests", UriKind.Relative);

    private readonly InMemoryDataRequestRepository _requests = new();
    private readonly InMemoryContactMessageRepository _messages = new();
    private readonly InMemoryTestimonialRepository _testimonials = new();
    private readonly WebApplicationFactory<Program> _factory;

    public DataRequestsControllersTests(KlfApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDataRequestRepository>();
            services.RemoveAll<IContactMessageRepository>();
            services.RemoveAll<ITestimonialRepository>();
            services.RemoveAll<ISiteSettingRepository>();
            services.RemoveAll<IUserDirectory>();
            services.RemoveAll<IPrivateFileStorage>();
            services.AddSingleton<IDataRequestRepository>(_requests);
            services.AddSingleton<IContactMessageRepository>(_messages);
            services.AddSingleton<ITestimonialRepository>(_testimonials);
            services.AddSingleton<ISiteSettingRepository>(new InMemorySiteSettingRepository());
            services.AddSingleton<IUserDirectory>(new InMemoryUserDirectory());
            services.AddSingleton<IPrivateFileStorage>(new FakePrivateFileStorage());
        }));
    }

    [Fact]
    public async Task Anyone_can_send_a_request_and_gets_the_protocol_and_deadline()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(PublicUri, new { name = "Ana", email = "ana@exemplo.com", kind = "Deletion" }, TestContext.Current.CancellationToken);
        var receipt = await response.Content.ReadFromJsonAsync<DataRequestReceiptResponse>(TestContext.Current.CancellationToken);
        var invalid = await client.PostAsJsonAsync(PublicUri, new { name = "Ana", email = "ana@exemplo.com", kind = "Correction" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Assert.Single(_requests.Requests).Protocol, receipt!.Protocol);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Only_the_administration_sees_requests_and_erases_data()
    {
        var request = new DataRequest("ABCD-2345", "Ana", "ana@exemplo.com", Domain.Enums.DataRequestKind.Deletion, null, DateTime.UtcNow);
        _requests.Add(request);
        _messages.Messages.Add(new ContactMessage("Ana", "ana@exemplo.com", null, null, null, null, "Mensagem antiga.", "2026-10-15", DateTime.UtcNow));
        using var anonymous = _factory.CreateClient();
        using var editor = CreateClientAs(Roles.Editor);
        using var admin = CreateClientAs(Roles.Admin);

        var unauthorized = await anonymous.GetAsync(AdminUri, TestContext.Current.CancellationToken);
        var forbidden = await editor.PostAsync(new Uri($"{AdminUri}/{request.Id}/erase-contact-messages", UriKind.Relative), null, TestContext.Current.CancellationToken);
        var findings = await admin.GetFromJsonAsync<DataRequestFindingsResponse>(new Uri($"{AdminUri}/{request.Id}/findings", UriKind.Relative), TestContext.Current.CancellationToken);
        var erased = await admin.PostAsync(new Uri($"{AdminUri}/{request.Id}/erase-contact-messages", UriKind.Relative), null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Single(findings!.ContactMessages);
        Assert.Equal(HttpStatusCode.OK, erased.StatusCode);
        Assert.Empty(_messages.Messages);
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
