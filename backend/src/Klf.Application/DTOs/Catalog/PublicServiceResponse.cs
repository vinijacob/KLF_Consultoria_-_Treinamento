using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Catalog;

/// <summary>An active service as its public detail page needs it.</summary>
/// <param name="Id">Service identifier.</param>
/// <param name="Title">Name of the service.</param>
/// <param name="Slug">URL identifier.</param>
/// <param name="Summary">Short description.</param>
/// <param name="ContentHtml">Detail page text as sanitized HTML.</param>
/// <param name="Audience">Who the service is meant for.</param>
/// <param name="WorkloadHours">Duration, in hours.</param>
/// <param name="Format">How it is delivered.</param>
/// <param name="CoverId">Cover image id.</param>
public sealed record PublicServiceResponse(
    Guid Id,
    string Title,
    string Slug,
    string? Summary,
    string ContentHtml,
    string Audience,
    int WorkloadHours,
    ServiceFormat Format,
    Guid? CoverId);
