using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Career;

/// <summary>An item of Kilciene's career timeline.</summary>
/// <param name="Id">Entry identifier.</param>
/// <param name="EntryType">Kind of entry: <c>Education</c>, <c>Certification</c> or <c>Experience</c>.</param>
/// <param name="Title">Degree, certification or job title.</param>
/// <param name="Institution">University, certifying body or company, if any.</param>
/// <param name="Description">Short description, if any.</param>
/// <param name="StartDate">When it started.</param>
/// <param name="EndDate">When it ended; <see langword="null"/> while ongoing.</param>
/// <param name="IsOngoing">Whether it is still ongoing (no end date).</param>
/// <param name="DisplayOrder">Position in the timeline; lower numbers come first.</param>
public sealed record CareerEntryResponse(
    Guid Id,
    CareerEntryType EntryType,
    string Title,
    string? Institution,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsOngoing,
    int DisplayOrder);
