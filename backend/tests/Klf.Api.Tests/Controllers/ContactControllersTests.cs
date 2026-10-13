using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Klf.Api.Tests.Fakes;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;
using Klf.Domain.Entities;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klf.Api.Tests.Controllers;

public sealed class ContactControllersTests : IClassFixture<KlfApiFactory>
{
    private static readonly Uri PublicContact = new("/api/v1/public/contact", UriKind.Relative);
    private static readonly Uri AdminMessages = new("/api/v1/admin/contact-messages", UriKind.Relative);

    private readonly InMemoryContactMessageRepository _messages = new();
    private readonly KlfApiFactory _baseFactory;
    private readonly WebApplicationFactory<Program> _factory;

    public ContactControllersTests(KlfApiFactory factory)
    {
        _baseFactory = factory;
        _factory = WithFakes(factory);
    }

    private WebApplicationFactory<Program> WithFakes(WebApplicationFactory<Program> factory, int? contactLimit = null) =>
        factory.WithWebHostBuilder(builder =>
        {
            if (contactLimit is { } limit)
            {
                builder.UseSetting("RateLimiting:ContactPer10Minutes", limit.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IContactMessageRepository>();
                services.RemoveAll<ISiteSettingRepository>();
                services.RemoveAll<IUnitOfWork>();
                services.AddSingleton<IContactMessageRepository>(_messages);
                services.AddSingleton<ISiteSettingRepository>(new EmptySettings());
                services.AddSingleton<IUnitOfWork>(_messages);
            });
        });

    private static object ValidBody(bool accepts = true) => new
    {
        name = "Ana Souza",
        email = "ana@loja.com.br",
        message = "Quero levar o treinamento para a minha equipe.",
        acceptsPrivacyPolicy = accepts,
        privacyPolicyVersion = "2026-10-12",
    };

    [Fact]
    public async Task Public_contact_accepts_a_valid_message_without_login_and_saves_it()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(PublicContact, ValidBody(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal("Ana Souza", Assert.Single(_messages.Messages).Name);
    }

    [Fact]
    public async Task Public_contact_returns_400_in_portuguese_when_the_privacy_policy_is_not_acknowledged()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(PublicContact, ValidBody(accepts: false), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("política de privacidade", body, StringComparison.Ordinal);
        Assert.Empty(_messages.Messages);
    }

    [Fact]
    public async Task Public_contact_returns_429_after_the_limit_per_ip()
    {
        using var limited = WithFakes(_baseFactory, contactLimit: 2);
        using var client = limited.CreateClient();

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            statuses.Add((await client.PostAsJsonAsync(PublicContact, ValidBody(), TestContext.Current.CancellationToken)).StatusCode);
        }

        Assert.Equal([HttpStatusCode.Accepted, HttpStatusCode.Accepted, HttpStatusCode.TooManyRequests], statuses);
    }

    [Fact]
    public async Task Admin_messages_need_login_and_are_admin_only()
    {
        using var anonymous = _factory.CreateClient();
        using var editor = CreateClientAs(Roles.Editor);

        var unauthorized = await anonymous.GetAsync(AdminMessages, TestContext.Current.CancellationToken);
        var forbidden = await editor.GetAsync(AdminMessages, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Admin_lists_marks_as_answered_and_deletes_a_message()
    {
        var message = new ContactMessage("Bruno", "bruno@loja.com.br", null, null, null, null, "Preciso de uma palestra.", "2026-10-12", DateTime.UtcNow);
        _messages.Messages.Add(message);
        using var admin = CreateClientAs(Roles.Admin);

        var list = await admin.GetStringAsync(AdminMessages, TestContext.Current.CancellationToken);
        var summary = await admin.GetStringAsync(new Uri($"{AdminMessages}/summary", UriKind.Relative), TestContext.Current.CancellationToken);
        var status = await admin.PutAsJsonAsync(new Uri($"{AdminMessages}/{message.Id}/status", UriKind.Relative), new { status = "Answered" }, TestContext.Current.CancellationToken);
        var deleted = await admin.DeleteAsync(new Uri($"{AdminMessages}/{message.Id}", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Contains("Bruno", list, StringComparison.Ordinal);
        Assert.Contains("\"newCount\":1", summary, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
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

    private sealed class EmptySettings : ISiteSettingRepository
    {
        public Task<IReadOnlyList<SiteSetting>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<SiteSetting>>([]);

        public Task<SiteSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken) => Task.FromResult<SiteSetting?>(null);

        public void Add(SiteSetting setting)
        {
        }
    }
}
