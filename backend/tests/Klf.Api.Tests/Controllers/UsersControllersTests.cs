using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Klf.Api.Tests.Fakes;
using Klf.Application.Interfaces.Identity;
using Klf.Domain.Common;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klf.Api.Tests.Controllers;

public sealed class UsersControllersTests : IClassFixture<KlfApiFactory>
{
    private static readonly Uri AdminUsers = new("/api/v1/admin/users", UriKind.Relative);
    private static readonly Uri AcceptInvite = new("/api/v1/auth/invite/accept", UriKind.Relative);

    private readonly InMemoryUserDirectory _directory = new();
    private readonly WebApplicationFactory<Program> _factory;

    public UsersControllersTests(KlfApiFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IUserDirectory>();
            services.AddSingleton<IUserDirectory>(_directory);
        }));
    }

    [Fact]
    public async Task Team_routes_need_login_and_are_admin_only()
    {
        using var anonymous = _factory.CreateClient();
        using var editor = CreateClientAs(Roles.Editor);

        var unauthorized = await anonymous.GetAsync(AdminUsers, TestContext.Current.CancellationToken);
        var forbidden = await editor.PostAsJsonAsync(AdminUsers, new { fullName = "X", email = "x@klf.test", role = "Admin" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Empty(_directory.Users);
    }

    [Fact]
    public async Task Admin_invites_and_the_person_accepts_once()
    {
        using var admin = CreateClientAs(Roles.Admin);
        using var visitor = _factory.CreateClient();

        var created = await admin.PostAsJsonAsync(AdminUsers, new { fullName = "Ana Editora", email = "ana@klf.test", role = "Editor" }, TestContext.Current.CancellationToken);
        var body = await created.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var accept = new { email = "ana@klf.test", token = InMemoryUserDirectory.ValidInviteToken, newPassword = "Senha@Forte123" };
        var first = await visitor.PostAsJsonAsync(AcceptInvite, accept, TestContext.Current.CancellationToken);
        var second = await visitor.PostAsJsonAsync(AcceptInvite, accept, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Contains("/painel/aceitar-convite?email=ana%40klf.test&token=", body, StringComparison.Ordinal);
        Assert.Contains("\"invitePending\":true", body, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Invite_with_an_unknown_role_returns_400_in_portuguese()
    {
        using var admin = CreateClientAs(Roles.Admin);

        var response = await admin.PostAsJsonAsync(AdminUsers, new { fullName = "Ana", email = "ana@klf.test", role = "Instructor" }, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Perfil inválido", body, StringComparison.Ordinal);
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
