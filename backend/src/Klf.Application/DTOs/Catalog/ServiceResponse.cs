using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Catalog;

/// <summary>A service with every field, for the admin panel.</summary>
/// <param name="Id">Service identifier.</param>
/// <param name="Title">Name of the service.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="Summary">Short description for cards.</param>
/// <param name="ContentHtml">Detail page text as sanitized HTML.</param>
/// <param name="Audience">Who the service is meant for.</param>
/// <param name="WorkloadHours">Duration, in hours.</param>
/// <param name="Format">How it is delivered.</param>
/// <param name="CoverId">Cover image id.</param>
/// <param name="DisplayOrder">Position in the public list.</param>
/// <param name="IsActive">Whether the service is visible on the public site.</param>
public sealed record ServiceResponse(
    Guid Id,
    string Title,
    string Slug,
    string? Summary,
    string ContentHtml,
    string Audience,
    int WorkloadHours,
    ServiceFormat Format,
    Guid? CoverId,
    int DisplayOrder,
    bool IsActive);
