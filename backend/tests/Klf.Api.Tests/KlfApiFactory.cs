using Klf.Api.Tests.Fakes;
using Klf.Application.Interfaces.Email;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Interfaces.Storage;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klf.Api.Tests;

/// <summary>
/// Runs the API in the <c>Testing</c> environment with a fake identity store and in-memory refresh tokens,
/// so tests never touch a real database.
/// </summary>
public class KlfApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@klf.test";
    public const string AdminPassword = "Senha@Forte123";

    public InMemoryRefreshTokenRepository RefreshTokens { get; } = new();

    public InMemoryMediaAssetRepository Media { get; } = new();

    public FakeFileStorage Storage { get; } = new();

    public FakeEmailSender Email { get; } = new();

    public FakeIdentityService Identity { get; } = new(new UserAccount(Guid.CreateVersion7(), AdminEmail, "Admin Teste", ["Admin"]));

    protected virtual bool UseInMemoryPersistence => true;

    public HttpClient CreateHttpsClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = "test-signing-key-that-is-long-enough-for-hmac-sha256",
            ["ConnectionStrings:Default"] = "Host=localhost;Database=klf_test_never_opened",
            ["RateLimiting:LoginPerMinute"] = "1000",
            ["TwoFactor:Required"] = "false",
        }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IIdentityService>();
            services.AddSingleton<IIdentityService>(Identity);
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Email);

            if (UseInMemoryPersistence)
            {
                services.RemoveAll<IRefreshTokenRepository>();
                services.RemoveAll<IUnitOfWork>();
                services.AddSingleton<IRefreshTokenRepository>(RefreshTokens);
                services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>();
                services.RemoveAll<IMediaAssetRepository>();
                services.RemoveAll<IFileStorage>();
                services.AddSingleton<IMediaAssetRepository>(Media);
                services.AddSingleton<IFileStorage>(Storage);
            }
        });
    }
}

/// <summary>Keeps the real EF Core registrations, for tests that check the dependency injection setup.</summary>
public sealed class RealPersistenceApiFactory : KlfApiFactory
{
    protected override bool UseInMemoryPersistence => false;
}
