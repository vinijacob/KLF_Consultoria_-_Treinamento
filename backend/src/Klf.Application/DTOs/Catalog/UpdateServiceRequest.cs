using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Catalog;

/// <summary>Data to replace a service.</summary>
/// <param name="Title">Name of the service (up to 200 characters).</param>
/// <param name="Slug">Unique URL identifier: lowercase letters, numbers and hyphens (up to 200 characters).</param>
/// <param name="Summary">Short description for cards (up to 500 characters); optional.</param>
/// <param name="ContentHtml">Detail page text as HTML; the server removes unsafe markup before saving.</param>
/// <param name="Audience">Who the service is meant for (up to 200 characters).</param>
/// <param name="WorkloadHours">Duration, in hours (1 to 2000).</param>
/// <param name="Format">How it is delivered: <c>InPerson</c>, <c>Online</c> or <c>InCompany</c>.</param>
/// <param name="CoverId">Cover image id; optional.</param>
/// <param name="DisplayOrder">Position in the public list; lower numbers come first.</param>
/// <param name="IsActive">Whether the service is visible on the public site.</param>
public sealed record UpdateServiceRequest(
    string Title,
    string Slug,
    string? Summary,
    string ContentHtml,
    string Audience,
    int WorkloadHours,
    ServiceFormat Format,
    Guid? CoverId,
    int DisplayOrder,
    bool IsActive) : IServiceFields;
