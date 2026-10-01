using System.Diagnostics.CodeAnalysis;

using Klf.Domain.Common;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Entities;

/// <summary>
/// An item of Kilciene's public career timeline (education, certification or experience), shown on the "Trajetória" page.
/// </summary>
public sealed class CareerEntry : SoftDeletableEntity
{
    /// <summary>Creates a career entry.</summary>
    /// <exception cref="ValidationException"><paramref name="endDate"/> is before <paramref name="startDate"/>.</exception>
    public CareerEntry(
        CareerEntryType entryType,
        string title,
        string? institution,
        string? description,
        DateOnly startDate,
        DateOnly? endDate,
        int displayOrder)
    {
        SetDetails(entryType, title, institution, description, startDate, endDate, displayOrder);
    }

    /// <summary>Kind of entry.</summary>
    public CareerEntryType EntryType { get; private set; }

    /// <summary>Main text: the degree, certification or job title (e.g. "MBA em Gestão de Pessoas").</summary>
    public string Title { get; private set; }

    /// <summary>University, certifying body or company; optional.</summary>
    public string? Institution { get; private set; }

    /// <summary>Short description shown under the title; optional.</summary>
    public string? Description { get; private set; }

    /// <summary>When it started.</summary>
    public DateOnly StartDate { get; private set; }

    /// <summary>When it ended; <see langword="null"/> while ongoing (e.g. current job).</summary>
    public DateOnly? EndDate { get; private set; }

    /// <summary>Position in the timeline; lower numbers come first.</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>Whether the entry is still ongoing.</summary>
    public bool IsOngoing => EndDate is null;

    /// <summary>Replaces every editable field.</summary>
    /// <exception cref="ValidationException"><paramref name="endDate"/> is before <paramref name="startDate"/>.</exception>
    public void Update(
        CareerEntryType entryType,
        string title,
        string? institution,
        string? description,
        DateOnly startDate,
        DateOnly? endDate,
        int displayOrder)
    {
        SetDetails(entryType, title, institution, description, startDate, endDate, displayOrder);
    }

    [MemberNotNull(nameof(Title))]
    private void SetDetails(
        CareerEntryType entryType,
        string title,
        string? institution,
        string? description,
        DateOnly startDate,
        DateOnly? endDate,
        int displayOrder)
    {
        if (endDate < startDate)
        {
            throw new ValidationException(nameof(EndDate), "A data de término não pode ser anterior à data de início.");
        }

        EntryType = entryType;
        Title = title;
        Institution = institution;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        DisplayOrder = displayOrder;
    }
}
