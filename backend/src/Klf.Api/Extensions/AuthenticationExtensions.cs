using System.Threading.RateLimiting;

using Klf.Api.Authorization;
using Klf.Infrastructure.Identity;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Klf.Api.Extensions;

/// <summary>Registers JWT Bearer authentication, the authorization policies and the rate limiting policies.</summary>
internal static class AuthenticationExtensions
{
    /// <summary>
    /// Rate limiting policy applied to login and refresh: <c>RateLimiting:LoginPerMinute</c> requests per minute per IP (default 10).
    /// </summary>
    public const string LoginRateLimitPolicy = "login";

    /// <summary>
    /// Validates incoming JWTs with the same <see cref="JwtOptions"/> used to issue them.
    /// Inbound claim mapping is disabled, so claims keep their short names (<c>sub</c>, <c>role</c>, <c>name</c>).
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwt.CreateSigningKey(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = ClaimTypeNames.Name,
                    RoleClaimType = ClaimTypeNames.Role,
                };
            });

        services.AddAuthorizationBuilder().AddKlfPolicies();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(LoginRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = context.RequestServices.GetRequiredService<IConfiguration>().GetValue("RateLimiting:LoginPerMinute", 10),
                    Window = TimeSpan.FromMinutes(1),
                }));
        });

        return services;
    }
}
