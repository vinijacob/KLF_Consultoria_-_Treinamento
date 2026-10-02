using Klf.Application.DTOs.Catalog;
using Klf.Domain.Entities;

namespace Klf.Application.Mappings;

internal static class ServiceMappings
{
    public static ServiceResponse ToResponse(this Service service) => new(
        service.Id,
        service.Title,
        service.Slug,
        service.Summary,
        service.ContentHtml,
        service.Audience,
        service.WorkloadHours,
        service.Format,
        service.CoverId,
        service.DisplayOrder,
        service.IsActive);

    public static ServiceListItemResponse ToListItem(this Service service) => new(
        service.Id,
        service.Title,
        service.Slug,
        service.Summary,
        service.WorkloadHours,
        service.Format,
        service.CoverId,
        service.DisplayOrder,
        service.IsActive);

    public static PublicServiceResponse ToPublicResponse(this Service service) => new(
        service.Id,
        service.Title,
        service.Slug,
        service.Summary,
        service.ContentHtml,
        service.Audience,
        service.WorkloadHours,
        service.Format,
        service.CoverId);
}
