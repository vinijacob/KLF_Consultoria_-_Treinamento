using Klf.Application.DTOs.Career;
using Klf.Domain.Entities;

namespace Klf.Application.Mappings;

internal static class CareerEntryMappings
{
    public static CareerEntryResponse ToResponse(this CareerEntry entry) => new(
        entry.Id,
        entry.EntryType,
        entry.Title,
        entry.Institution,
        entry.Description,
        entry.StartDate,
        entry.EndDate,
        entry.IsOngoing,
        entry.DisplayOrder);
}
