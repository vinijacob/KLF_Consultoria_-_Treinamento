using Klf.Application.Interfaces.Content;
using Klf.Application.Interfaces.Documents;
using Klf.Application.Interfaces.Email;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Links;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Interfaces.Storage;
using Klf.Infrastructure.Documents;
using Klf.Infrastructure.Email;
using Klf.Infrastructure.Identity;
using Klf.Infrastructure.Persistence;
using Klf.Infrastructure.Repositories;
using Klf.Infrastructure.Services;
using Klf.Infrastructure.Storage;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

        services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromMinutes(30));

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.SectionName));
        services.AddSingleton<EmailQueue>();
        services.AddSingleton<IEmailSender, QueuedEmailSender>();
        services.AddSingleton<IFrontendLinks, FrontendLinks>();
        services.AddHostedService<EmailDispatcher>();
        services.AddHttpClient<ResendEmailTransport>(client => client.BaseAddress = new Uri("https://api.resend.com/"));
        services.AddScoped<IEmailTransport>(sp =>
            sp.GetRequiredService<IOptions<EmailOptions>>().Value.Provider switch
            {
                EmailOptions.ResendProvider => sp.GetRequiredService<ResendEmailTransport>(),
                EmailOptions.LogProvider => ActivatorUtilities.CreateInstance<LogEmailTransport>(sp),
                var other => throw new InvalidOperationException($"Email:Provider inválido: '{other}'. Use Log ou Resend."),
            });

        services.Configure<TwoFactorOptions>(configuration.GetSection(TwoFactorOptions.SectionName));
        services.AddSingleton<ITwoFactorPolicy, TwoFactorPolicy>();

        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IHtmlContentSanitizer, HtmlContentSanitizer>();
        services.AddHostedService<AdminSeeder>();

        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IFileStorage>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;

            return options.Provider switch
            {
                StorageOptions.R2Provider => new R2FileStorage(options),
                StorageOptions.LocalProvider => new LocalFileStorage(
                    Path.Combine(sp.GetRequiredService<IHostEnvironment>().ContentRootPath, options.LocalDirectory),
                    options.PublicBaseUrl),
                _ => throw new InvalidOperationException($"Storage:Provider inválido: '{options.Provider}'. Use Local ou R2."),
            };
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICareerEntryRepository, CareerEntryRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ISiteSettingRepository, SiteSettingRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ITestimonialRepository, TestimonialRepository>();
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<IAlbumRepository, AlbumRepository>();
        services.AddScoped<IFeedbackFormRepository, FeedbackFormRepository>();
        services.AddScoped<IFeedbackSessionRepository, FeedbackSessionRepository>();
        services.AddScoped<IFeedbackResponseRepository, FeedbackResponseRepository>();
        services.AddSingleton<IFeedbackPosterRenderer, FeedbackPosterRenderer>();

        return services;
    }
}
