using FluentValidation;

using Klf.Application.Services.Auth;
using Klf.Application.Services.Career;

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

        return services;
    }
}
