using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Catalog;

/// <summary>A service in a list: no detail text, so lists stay small.</summary>
/// <param name="Id">Service identifier.</param>
/// <param name="Title">Name of the service.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="Summary">Short description for cards.</param>
/// <param name="WorkloadHours">Duration, in hours.</param>
/// <param name="Format">How it is delivered.</param>
/// <param name="CoverId">Cover image id.</param>
/// <param name="DisplayOrder">Position in the list.</param>
/// <param name="IsActive">Whether the service is visible on the public site.</param>
public sealed record ServiceListItemResponse(
    Guid Id,
    string Title,
    string Slug,
    string? Summary,
    int WorkloadHours,
    ServiceFormat Format,
    Guid? CoverId,
    int DisplayOrder,
    bool IsActive);
