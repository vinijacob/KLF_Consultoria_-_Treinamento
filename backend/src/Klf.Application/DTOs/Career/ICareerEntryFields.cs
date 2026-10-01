using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Career;

/// <summary>Editable fields shared by the create and update requests, so both use the same validation rules.</summary>
public interface ICareerEntryFields
{
    /// <summary>Kind of entry.</summary>
    CareerEntryType EntryType { get; }

    /// <summary>Degree, certification or job title.</summary>
    string Title { get; }

    /// <summary>University, certifying body or company.</summary>
    string? Institution { get; }

    /// <summary>Short description.</summary>
    string? Description { get; }

    /// <summary>When it started.</summary>
    DateOnly StartDate { get; }

    /// <summary>When it ended; <see langword="null"/> while ongoing.</summary>
    DateOnly? EndDate { get; }

    /// <summary>Position in the timeline.</summary>
    int DisplayOrder { get; }
}
