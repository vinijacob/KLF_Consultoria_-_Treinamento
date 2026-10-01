using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Career;

/// <summary>Data to add an item to Kilciene's career timeline.</summary>
/// <param name="EntryType">Kind of entry: <c>Education</c>, <c>Certification</c> or <c>Experience</c>.</param>
/// <param name="Title">Degree, certification or job title (up to 150 characters).</param>
/// <param name="Institution">University, certifying body or company (up to 150 characters); optional.</param>
/// <param name="Description">Short description (up to 1000 characters); optional.</param>
/// <param name="StartDate">When it started (<c>yyyy-MM-dd</c>).</param>
/// <param name="EndDate">When it ended (<c>yyyy-MM-dd</c>); leave empty while ongoing.</param>
/// <param name="DisplayOrder">Position in the timeline; lower numbers come first.</param>
public sealed record CreateCareerEntryRequest(
    CareerEntryType EntryType,
    string Title,
    string? Institution,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    int DisplayOrder) : ICareerEntryFields;
