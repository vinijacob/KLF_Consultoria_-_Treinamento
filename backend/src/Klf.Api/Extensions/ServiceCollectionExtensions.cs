using Klf.Api.Filters;

namespace Klf.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public const string FrontendCorsPolicy = "Frontend";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options => options.Filters.Add<ValidationFilter>());
        services.AddOpenApi();
        services.AddProblemDetails();

        services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
            policy
                .WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        return services;
    }
}
