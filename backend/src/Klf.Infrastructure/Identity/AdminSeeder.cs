using Klf.Domain.Common;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Klf.Infrastructure.Identity;

/// <summary>
/// Creates every role in <see cref="Roles.All"/> and the initial admin account on startup, using <see cref="AdminSeedOptions"/>.
/// Does nothing when the seed is not configured, and never changes an account that already exists.
/// </summary>
internal sealed partial class AdminSeeder(
    IServiceScopeFactory scopeFactory,
    IOptions<AdminSeedOptions> options,
    ILogger<AdminSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var seed = options.Value;

        if (string.IsNullOrWhiteSpace(seed.Email) || string.IsNullOrWhiteSpace(seed.Password))
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole<Guid>(role) { Id = Guid.CreateVersion7() }));
            }
        }

        var user = await userManager.FindByEmailAsync(seed.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = seed.Email,
                Email = seed.Email,
                EmailConfirmed = true,
                FullName = seed.FullName,
            };

            EnsureSucceeded(await userManager.CreateAsync(user, seed.Password));
            LogAdminCreated(logger);
        }

        if (!await userManager.IsInRoleAsync(user, Roles.Admin))
        {
            EnsureSucceeded(await userManager.AddToRoleAsync(user, Roles.Admin));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Falha ao criar o admin inicial: {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Initial admin account created")]
    private static partial void LogAdminCreated(ILogger logger);
}
