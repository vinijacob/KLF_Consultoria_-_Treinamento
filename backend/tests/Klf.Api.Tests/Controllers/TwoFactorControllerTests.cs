using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Klf.Api.Tests.Fakes;
using Klf.Application.DTOs.Auth;
using Klf.Application.Interfaces.Identity;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klf.Api.Tests.Controllers;

public sealed class TwoFactorControllerTests : IDisposable
{
    private static readonly Uri LoginUri = new("/api/v1/auth/login", UriKind.Relative);
    private static readonly Uri VerifyUri = new("/api/v1/auth/2fa/verify", UriKind.Relative);
    private static readonly Uri RecoverUri = new("/api/v1/auth/2fa/recover", UriKind.Relative);
    private static readonly Uri SetupUri = new("/api/v1/auth/2fa/setup", UriKind.Relative);
    private static readonly Uri EnableUri = new("/api/v1/auth/2fa/enable", UriKind.Relative);
    private static readonly Uri CodesUri = new("/api/v1/auth/2fa/recovery-codes", UriKind.Relative);
    private static readonly Uri MeUri = new("/api/v1/auth/me", UriKind.Relative);

    private readonly FakeIdentityService _identity = new(new UserAccount(Guid.CreateVersion7(), KlfApiFactory.AdminEmail, "Admin", ["Admin"]));
    private readonly KlfApiFactory _root = new();
    private readonly WebApplicationFactory<Program> _factory;

    public TwoFactorControllerTests()
    {
        _factory = _root.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?> { ["TwoFactor:Required"] = "true" }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IIdentityService>();
                services.AddSingleton<IIdentityService>(_identity);
            });
        });
    }

    [Fact]
    public async Task Login_returns_setup_required_with_token_and_no_cookie_when_user_is_not_enrolled()
    {
        using var client = CreateHttpsClient();

        var response = await client.PostAsJsonAsync(LoginUri, Credentials(), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body!.TwoFactorSetupRequired);
        Assert.False(body.TwoFactorRequired);
        Assert.Null(body.AccessToken);
        Assert.False(string.IsNullOrWhiteSpace(body.TwoFactorToken));
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Enrollment_flow_sets_up_enables_and_returns_session_with_ten_recovery_codes()
    {
        using var client = CreateHttpsClient();
        var challenge = await LoginAsync(client);

        var setup = await client.PostAsJsonAsync(SetupUri, new TwoFactorSetupRequest(challenge.TwoFactorToken!), TestContext.Current.CancellationToken);
        var setupBody = await setup.Content.ReadFromJsonAsync<TwoFactorSetupResponse>(TestContext.Current.CancellationToken);
        var enable = await client.PostAsJsonAsync(EnableUri, new TwoFactorCodeRequest(challenge.TwoFactorToken!, FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);
        var enableBody = await enable.Content.ReadFromJsonAsync<TwoFactorEnabledResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, setup.StatusCode);
        Assert.StartsWith("otpauth://totp/", setupBody!.OtpAuthUri, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, enable.StatusCode);
        Assert.Equal(10, enableBody!.RecoveryCodes.Count);
        Assert.Contains(enable.Headers.GetValues("Set-Cookie"), c => c.StartsWith("klf_refresh=", StringComparison.Ordinal) && c.Contains("httponly", StringComparison.OrdinalIgnoreCase));
        Assert.True(_identity.TwoFactorEnabled);
    }

    [Fact]
    public async Task Enable_returns_401_when_code_is_wrong()
    {
        using var client = CreateHttpsClient();
        var challenge = await LoginAsync(client);

        var response = await client.PostAsJsonAsync(EnableUri, new TwoFactorCodeRequest(challenge.TwoFactorToken!, "000000"), TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Código inválido.", problem!.Detail);
        Assert.False(_identity.TwoFactorEnabled);
    }

    [Fact]
    public async Task Login_then_verify_returns_session_when_user_is_enrolled_and_code_is_valid()
    {
        _identity.TwoFactorEnabled = true;
        using var client = CreateHttpsClient();

        var challenge = await LoginAsync(client);
        var verify = await client.PostAsJsonAsync(VerifyUri, new TwoFactorCodeRequest(challenge.TwoFactorToken!, FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);
        var session = await verify.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        Assert.True(challenge.TwoFactorRequired);
        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(session!.AccessToken));
        Assert.Contains(verify.Headers.GetValues("Set-Cookie"), c => c.StartsWith("klf_refresh=", StringComparison.Ordinal));

        using var authed = _factory.CreateClient();
        authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        var me = await authed.GetFromJsonAsync<CurrentUserResponse>(MeUri, TestContext.Current.CancellationToken);
        Assert.True(me!.TwoFactorEnabled);
    }

    [Fact]
    public async Task Verify_returns_401_for_wrong_code_and_for_garbage_token()
    {
        _identity.TwoFactorEnabled = true;
        using var client = CreateHttpsClient();
        var challenge = await LoginAsync(client);

        var wrong = await client.PostAsJsonAsync(VerifyUri, new TwoFactorCodeRequest(challenge.TwoFactorToken!, "000000"), TestContext.Current.CancellationToken);
        var garbage = await client.PostAsJsonAsync(VerifyUri, new TwoFactorCodeRequest("nao-e-um-token", FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, garbage.StatusCode);
    }

    [Fact]
    public async Task Access_token_cannot_be_used_as_two_factor_token_and_challenge_cannot_open_admin_routes()
    {
        _identity.TwoFactorEnabled = true;
        using var client = CreateHttpsClient();
        var challenge = await LoginAsync(client);
        var access = _factory.Services.GetRequiredService<ITokenService>()
            .Generate(new UserAccount(Guid.CreateVersion7(), "a@klf.test", "A", ["Admin"])).Token;

        var asTwoFactor = await client.PostAsJsonAsync(VerifyUri, new TwoFactorCodeRequest(access, FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);
        using var withChallenge = _factory.CreateClient();
        withChallenge.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", challenge.TwoFactorToken);
        var admin = await withChallenge.GetAsync(new Uri("/api/v1/admin/posts", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, asTwoFactor.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, admin.StatusCode);
    }

    [Fact]
    public async Task Recover_works_once_then_returns_401_for_the_same_code()
    {
        _identity.TwoFactorEnabled = true;
        using var client = CreateHttpsClient();
        var challenge = await LoginAsync(client);
        var request = new TwoFactorRecoveryRequest(challenge.TwoFactorToken!, FakeIdentityService.ValidRecoveryCode);

        var first = await client.PostAsJsonAsync(RecoverUri, request, TestContext.Current.CancellationToken);
        var second = await client.PostAsJsonAsync(RecoverUri, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
    }

    [Fact]
    public async Task Setup_and_enable_return_409_when_two_factor_is_already_enabled()
    {
        _identity.TwoFactorEnabled = true;
        using var client = CreateHttpsClient();
        var challenge = await LoginAsync(client);

        var setup = await client.PostAsJsonAsync(SetupUri, new TwoFactorSetupRequest(challenge.TwoFactorToken!), TestContext.Current.CancellationToken);
        var enable = await client.PostAsJsonAsync(EnableUri, new TwoFactorCodeRequest(challenge.TwoFactorToken!, FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, setup.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, enable.StatusCode);
    }

    [Fact]
    public async Task Second_step_returns_400_in_portuguese_when_code_is_malformed()
    {
        using var client = CreateHttpsClient();

        var response = await client.PostAsJsonAsync(VerifyUri, new TwoFactorCodeRequest("token", "12"), TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["O código deve ter 6 dígitos."], problem!.Errors["Code"]);
    }

    [Fact]
    public async Task Recovery_codes_endpoint_requires_login_and_returns_new_codes_when_code_is_valid()
    {
        _identity.TwoFactorEnabled = true;
        using var client = CreateHttpsClient();
        var challenge = await LoginAsync(client);
        var verify = await client.PostAsJsonAsync(VerifyUri, new TwoFactorCodeRequest(challenge.TwoFactorToken!, FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);
        var session = await verify.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        using var anonymous = _factory.CreateClient();
        var unauthorized = await anonymous.PostAsJsonAsync(CodesUri, new RegenerateRecoveryCodesRequest(FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);
        using var authed = _factory.CreateClient();
        authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.AccessToken);
        var ok = await authed.PostAsJsonAsync(CodesUri, new RegenerateRecoveryCodesRequest(FakeIdentityService.ValidCode), TestContext.Current.CancellationToken);
        var body = await ok.Content.ReadFromJsonAsync<RecoveryCodesResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(10, body!.RecoveryCodes.Count);
    }

    public void Dispose()
    {
        _factory.Dispose();
        _root.Dispose();
    }

    private HttpClient CreateHttpsClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    private static LoginRequest Credentials() => new(KlfApiFactory.AdminEmail, KlfApiFactory.AdminPassword);

    private static async Task<LoginResponse> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(LoginUri, Credentials(), TestContext.Current.CancellationToken);

        return (await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken))!;
    }
}
