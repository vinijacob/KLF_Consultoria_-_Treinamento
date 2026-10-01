using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Klf.Api.Extensions;

/// <summary>Adds the Bearer security scheme to the OpenAPI document so Scalar can send the JWT.</summary>
internal static class OpenApiExtensions
{
    /// <summary>Name of the security scheme shown in Scalar.</summary>
    public const string BearerScheme = "Bearer";

    /// <summary>
    /// Declares the Bearer scheme and marks every operation that requires authorization with it,
    /// which shows the lock icon in Scalar.
    /// </summary>
    public static OpenApiOptions AddBearerSecurity(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Token JWT obtido em POST /api/v1/auth/login.",
            };

            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, _) =>
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;

            if (metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any())
            {
                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(BearerScheme, context.Document)] = [],
                });
            }

            return Task.CompletedTask;
        });

        return options;
    }
}
