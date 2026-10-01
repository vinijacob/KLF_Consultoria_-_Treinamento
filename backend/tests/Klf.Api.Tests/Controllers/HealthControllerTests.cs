using System.Net;
using System.Net.Http.Json;

using Klf.Application.DTOs.Health;

namespace Klf.Api.Tests.Controllers;

public sealed class HealthControllerTests(KlfApiFactory factory) : IClassFixture<KlfApiFactory>
{
    [Fact]
    public async Task Health_returns_ok_when_api_is_running()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v1/public/health", UriKind.Relative), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", body?.Status);
    }
}
