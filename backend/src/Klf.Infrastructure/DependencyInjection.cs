using Klf.Application.Interfaces.Content;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Repositories;
using Klf.Infrastructure.Identity;
using Klf.Infrastructure.Persistence;
using Klf.Infrastructure.Repositories;
using Klf.Infrastructure.Services;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Klf.Infrastructure;

/// <summary>Registers the Infrastructure layer services.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the PostgreSQL <see cref="AppDbContext"/>, ASP.NET Core Identity, JWT issuing, the repositories and the external service integrations.
    /// </summary>
    /// <remarks>
    /// The connection string is read from <c>ConnectionStrings:Default</c> only when the DbContext is first created,
    /// so the app (and tests that don't touch the database) can start without it.
    /// </remarks>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException(
                    "Connection string 'Default' não configurada. Em desenvolvimento, use: dotnet user-secrets set \"ConnectionStrings:Default\" \"...\" --project src/Klf.Api");

            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddDataProtection();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 10;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IHtmlContentSanitizer, HtmlContentSanitizer>();
        services.AddHostedService<AdminSeeder>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICareerEntryRepository, CareerEntryRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        return services;
    }
}
