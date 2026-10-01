namespace Klf.Application.DTOs.Health;

/// <summary>API health status.</summary>
/// <param name="Status">Always <c>ok</c> when the API is running.</param>
public sealed record HealthResponse(string Status);
