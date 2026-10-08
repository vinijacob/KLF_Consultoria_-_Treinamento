using System.Net;
using System.Net.Http.Json;

using Klf.Api.Tests.Fakes;
using Klf.Application.DTOs.Auth;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Tests.Controllers;

public sealed class PasswordRecoveryControllerTests : IDisposable
{
    private static readonly Uri ForgotUri = new("/api/v1/auth/forgot", UriKind.Relative);
    private static readonly Uri ResetUri = new("/api/v1/auth/reset", UriKind.Relative);
    private static readonly Uri LoginUri = new("/api/v1/auth/login", UriKind.Relative);

    private readonly KlfApiFactory _factory = new();

    [Fact]
    public async Task Forgot_returns_202_and_queues_one_email_when_account_exists()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(ForgotUri, new ForgotPasswordRequest(KlfApiFactory.AdminEmail), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var message = Assert.Single(_factory.Email.Sent);
        Assert.Contains("token=reset-token", message.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Forgot_returns_the_same_202_with_empty_body_and_sends_nothing_when_account_does_not_exist()
    {
        using var client = _factory.CreateClient();

        var known = await client.PostAsJsonAsync(ForgotUri, new ForgotPasswordRequest(KlfApiFactory.AdminEmail), TestContext.Current.CancellationToken);
        var unknown = await client.PostAsJsonAsync(ForgotUri, new ForgotPasswordRequest("ninguem@klf.test"), TestContext.Current.CancellationToken);

        Assert.Equal(known.StatusCode, unknown.StatusCode);
        Assert.Equal(await known.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), await unknown.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Single(_factory.Email.Sent);
    }

    [Fact]
    public async Task Forgot_returns_400_in_portuguese_when_email_is_invalid()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(ForgotUri, new ForgotPasswordRequest("nao-e-email"), TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["Informe um e-mail válido."], problem!.Errors["Email"]);
    }

    [Fact]
    public async Task Reset_changes_password_so_old_one_stops_and_new_one_logs_in_and_token_works_once()
    {
        using var client = _factory.CreateClient();
        var request = new ResetPasswordRequest(KlfApiFactory.AdminEmail, FakeIdentityService.ValidResetToken, "Nova@Senha1234");

        var reset = await client.PostAsJsonAsync(ResetUri, request, TestContext.Current.CancellationToken);
        var oldLogin = await client.PostAsJsonAsync(LoginUri, new LoginRequest(KlfApiFactory.AdminEmail, KlfApiFactory.AdminPassword), TestContext.Current.CancellationToken);
        var newLogin = await client.PostAsJsonAsync(LoginUri, new LoginRequest(KlfApiFactory.AdminEmail, "Nova@Senha1234"), TestContext.Current.CancellationToken);
        var again = await client.PostAsJsonAsync(ResetUri, request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
        Assert.Contains(_factory.Email.Sent, m => m.Subject.Contains("alterada", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Reset_returns_400_with_clear_message_when_link_is_invalid_or_password_is_weak()
    {
        using var client = _factory.CreateClient();

        var invalid = await client.PostAsJsonAsync(ResetUri, new ResetPasswordRequest(KlfApiFactory.AdminEmail, "errado", "Nova@Senha1234"), TestContext.Current.CancellationToken);
        var invalidProblem = await invalid.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        var weak = await client.PostAsJsonAsync(ResetUri, new ResetPasswordRequest(KlfApiFactory.AdminEmail, FakeIdentityService.ValidResetToken, "curta"), TestContext.Current.CancellationToken);
        var weakProblem = await weak.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(["Link inválido ou expirado. Solicite um novo."], invalidProblem!.Errors["Token"]);
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);
        Assert.Equal(["A senha deve ter pelo menos 10 caracteres."], weakProblem!.Errors["NewPassword"]);
    }

    public void Dispose() => _factory.Dispose();
}
