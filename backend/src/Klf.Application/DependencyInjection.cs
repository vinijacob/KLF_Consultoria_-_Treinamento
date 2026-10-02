using FluentValidation;

using Klf.Application.DTOs;
using Klf.Application.Services.Auth;
using Klf.Application.Services.Career;
using Klf.Application.Services.Catalog;
using Klf.Application.Services.Posts;
using Klf.Application.Services.Settings;

using Microsoft.Extensions.DependencyInjection;

namespace Klf.Application;

/// <summary>Registers the Application layer services.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application services and every FluentValidation validator found in this assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICareerEntryService, CareerEntryService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<ISiteSettingService, SiteSettingService>();

        return services;
    }
}
