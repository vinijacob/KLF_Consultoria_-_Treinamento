using System.Text.Json.Serialization;

using Klf.Api.Filters;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Extensions;

/// <summary>Registers the API (presentation) layer services.</summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>Name of the CORS policy that allows the origins listed in <c>Cors:AllowedOrigins</c>.</summary>
    public const string FrontendCorsPolicy = "Frontend";

    /// <summary>
    /// Registers controllers (with <see cref="ValidationFilter"/>), OpenAPI, ProblemDetails, CORS and JWT authentication.
    /// </summary>
    /// <remarks>
    /// The implicit <c>[Required]</c> on non-nullable properties is disabled so every validation message
    /// comes from FluentValidation, in Portuguese. Enums are read and written as text (e.g. <c>"Education"</c>).
    /// </remarks>
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddOpenApi(options => options.AddBearerSecurity());
        services.AddProblemDetails();
        services.AddJwtAuthentication();
        services.AddSingleton<FeedbackCookie>();

        services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
            policy
                .WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("Content-Disposition")
                .AllowCredentials()));

        return services;
    }
}
