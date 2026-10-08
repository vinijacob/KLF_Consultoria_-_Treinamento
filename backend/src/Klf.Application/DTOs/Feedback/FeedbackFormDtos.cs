namespace Klf.Application.DTOs.Feedback;

/// <summary>Editable fields shared by the create and update requests of a form template.</summary>
public interface IFeedbackFormFields
{
    /// <summary>Name of the template, also the title respondents see.</summary>
    string Title { get; }

    /// <summary>Text shown at the top of the form.</summary>
    string? Description { get; }

    /// <summary>Sections and questions.</summary>
    FormDefinitionDto Definition { get; }
}

/// <summary>Data to create a feedback form template.</summary>
/// <param name="Title">Name of the template, also the title respondents see (up to 200 characters).</param>
/// <param name="Description">Text shown at the top of the form (up to 1000 characters); optional.</param>
/// <param name="Definition">Sections and questions.</param>
public sealed record CreateFeedbackFormRequest(string Title, string? Description, FormDefinitionDto Definition) : IFeedbackFormFields;

/// <summary>Data to replace a feedback form template. Sessions already created keep their own copy.</summary>
/// <param name="Title">Name of the template (up to 200 characters).</param>
/// <param name="Description">Text shown at the top of the form (up to 1000 characters); optional.</param>
/// <param name="Definition">Sections and questions.</param>
public sealed record UpdateFeedbackFormRequest(string Title, string? Description, FormDefinitionDto Definition) : IFeedbackFormFields;

/// <summary>A form template with its questions.</summary>
/// <param name="Id">Template identifier.</param>
/// <param name="Title">Name of the template.</param>
/// <param name="Description">Text shown at the top of the form.</param>
/// <param name="Definition">Sections and questions.</param>
public sealed record FeedbackFormResponse(Guid Id, string Title, string? Description, FormDefinitionDto Definition);

/// <summary>A form template in a list.</summary>
/// <param name="Id">Template identifier.</param>
/// <param name="Title">Name of the template.</param>
/// <param name="Description">Text shown at the top of the form.</param>
/// <param name="SectionCount">Number of sections.</param>
/// <param name="QuestionCount">Number of questions.</param>
public sealed record FeedbackFormListItemResponse(Guid Id, string Title, string? Description, int SectionCount, int QuestionCount);
