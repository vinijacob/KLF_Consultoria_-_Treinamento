using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Klf.Application.DTOs.Auth;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Tests.Controllers;

public sealed class AuthControllerTests(KlfApiFactory factory) : IClassFixture<KlfApiFactory>
{
    private static readonly Uri LoginUri = new("/api/v1/auth/login", UriKind.Relative);
    private static readonly Uri MeUri = new("/api/v1/auth/me", UriKind.Relative);
    private static readonly Uri RefreshUri = new("/api/v1/auth/refresh", UriKind.Relative);
    private static readonly Uri LogoutUri = new("/api/v1/auth/logout", UriKind.Relative);

    [Fact]
    public async Task Login_returns_token_when_credentials_are_valid()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(LoginUri, new LoginRequest(KlfApiFactory.AdminEmail, KlfApiFactory.AdminPassword), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body?.AccessToken));
        Assert.True(body.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_returns_401_when_password_is_wrong()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(LoginUri, new LoginRequest(KlfApiFactory.AdminEmail, "senha-errada"), TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("E-mail ou senha inválidos.", problem?.Detail);
    }

    [Fact]
    public async Task Login_returns_400_with_portuguese_messages_when_body_is_empty()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(LoginUri, new { }, TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(["Informe o e-mail."], problem.Errors["Email"]);
        Assert.Equal(["Informe a senha."], problem.Errors["Password"]);
    }

    [Fact]
    public async Task Me_returns_401_when_token_is_missing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(MeUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_returns_401_when_token_is_tampered()
    {
        using var client = factory.CreateClient();
        var token = await LoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token[..^4] + "abcd");

        var response = await client.GetAsync(MeUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_returns_current_user_when_token_is_valid()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync(MeUri, TestContext.Current.CancellationToken);
        var user = await response.Content.ReadFromJsonAsync<CurrentUserResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(KlfApiFactory.AdminEmail, user?.Email);
        Assert.Equal(["Admin"], user?.Roles);
    }

    [Fact]
    public async Task Login_sets_http_only_refresh_cookie_when_credentials_are_valid()
    {
        using var client = factory.CreateHttpsClient();

        var response = await client.PostAsJsonAsync(LoginUri, new LoginRequest(KlfApiFactory.AdminEmail, KlfApiFactory.AdminPassword), TestContext.Current.CancellationToken);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));

        Assert.StartsWith("klf_refresh=", cookie, StringComparison.Ordinal);
        Assert.Contains("path=/api/v1/auth", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Refresh_returns_new_access_token_when_cookie_is_valid()
    {
        using var client = factory.CreateHttpsClient();
        await LoginAsync(client);

        var response = await client.PostAsync(RefreshUri, content: null, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body?.AccessToken));
    }

    [Fact]
    public async Task Refresh_returns_401_when_cookie_is_missing()
    {
        using var client = factory.CreateHttpsClient();

        var response = await client.PostAsync(RefreshUri, content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_returns_401_when_old_cookie_is_reused_after_rotation()
    {
        using var client = factory.CreateHttpsClient();
        var login = await client.PostAsJsonAsync(LoginUri, new LoginRequest(KlfApiFactory.AdminEmail, KlfApiFactory.AdminPassword), TestContext.Current.CancellationToken);
        var oldCookie = login.Headers.GetValues("Set-Cookie").Single().Split(';')[0];
        await client.PostAsync(RefreshUri, content: null, TestContext.Current.CancellationToken);

        using var attacker = factory.CreateHttpsClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, RefreshUri);
        request.Headers.Add("Cookie", oldCookie);
        var response = await attacker.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_deletes_cookie_and_blocks_refresh_when_session_is_active()
    {
        using var client = factory.CreateHttpsClient();
        await LoginAsync(client);

        var logout = await client.PostAsync(LogoutUri, content: null, TestContext.Current.CancellationToken);
        var refresh = await client.PostAsync(RefreshUri, content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Contains("klf_refresh=;", Assert.Single(logout.Headers.GetValues("Set-Cookie")), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Logout_returns_204_when_there_is_no_cookie()
    {
        using var client = factory.CreateHttpsClient();

        var response = await client.PostAsync(LogoutUri, content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(LoginUri, new LoginRequest(KlfApiFactory.AdminEmail, KlfApiFactory.AdminPassword), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        return body!.AccessToken;
    }
}
