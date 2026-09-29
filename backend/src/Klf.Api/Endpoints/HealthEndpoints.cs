namespace Klf.Api.Endpoints;

internal static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/health", () => TypedResults.Ok(new HealthResponse("ok")))
            .WithName("GetHealth")
            .WithTags("Health");

        return api;
    }
}

internal sealed record HealthResponse(string Status);
