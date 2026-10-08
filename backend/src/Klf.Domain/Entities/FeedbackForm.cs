using System.Diagnostics.CodeAnalysis;

using Klf.Domain.Common;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Entities;

/// <summary>
/// A reusable feedback form template built in the admin panel. Opening a session copies its <see cref="Definition"/>,
/// so later edits here never affect sessions that already exist.
/// </summary>
public sealed class FeedbackForm : SoftDeletableEntity
{
    /// <summary>Creates a template.</summary>
    /// <param name="title">Name of the template (also the title respondents see).</param>
    /// <param name="description">Text shown at the top of the form; optional.</param>
    /// <param name="definition">Sections and questions.</param>
    /// <exception cref="ValidationException">The definition is invalid.</exception>
    public FeedbackForm(string title, string? description, FormDefinition definition)
    {
        Update(title, description, definition);
    }

    private FeedbackForm()
    {
        Title = null!;
        Definition = null!;
    }

    /// <summary>Name of the template, also the title respondents see.</summary>
    public string Title { get; private set; }

    /// <summary>Text shown at the top of the form; optional.</summary>
    public string? Description { get; private set; }

    /// <summary>Sections and questions.</summary>
    public FormDefinition Definition { get; private set; }

    /// <summary>Replaces every editable field.</summary>
    /// <param name="title">Name of the template.</param>
    /// <param name="description">Text shown at the top of the form; optional.</param>
    /// <param name="definition">Sections and questions.</param>
    /// <exception cref="ValidationException">The definition is invalid.</exception>
    [MemberNotNull(nameof(Title), nameof(Definition))]
    public void Update(string title, string? description, FormDefinition definition)
    {
        definition.EnsureValid();

        Title = title;
        Description = description;
        Definition = definition;
    }
}
