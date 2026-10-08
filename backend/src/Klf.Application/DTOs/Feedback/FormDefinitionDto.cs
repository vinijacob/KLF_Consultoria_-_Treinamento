using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Feedback;

/// <summary>Sections and questions of a feedback form, as built in the admin panel.</summary>
/// <param name="Sections">Sections in display order (1 to 20; up to 100 questions in total).</param>
public sealed record FormDefinitionDto(IReadOnlyList<FormSectionDto> Sections);

/// <summary>A section of a feedback form.</summary>
/// <param name="Id">Identifier chosen by the builder: 1 to 40 letters, numbers, '-' or '_'; unique in the form.</param>
/// <param name="Title">Section title (up to 200 characters).</param>
/// <param name="Description">Text under the title (up to 1000 characters); optional.</param>
/// <param name="Topic"><c>Company</c> (the trainee's company) or <c>Training</c> (KLF and the training). Results are grouped by it.</param>
/// <param name="Questions">Questions in display order.</param>
public sealed record FormSectionDto(string Id, string Title, string? Description, FeedbackTopic Topic, IReadOnlyList<FormQuestionDto> Questions);

/// <summary>A question of a feedback form.</summary>
/// <param name="Id">Identifier chosen by the builder, unique in the form; answers and conditions refer to it.</param>
/// <param name="Type"><c>ShortText</c>, <c>LongText</c>, <c>Scale</c>, <c>Nps</c> (0 to 10), <c>SingleChoice</c> or <c>MultipleChoice</c>.</param>
/// <param name="Text">The question (up to 500 characters).</param>
/// <param name="HelpText">Extra explanation (up to 500 characters); optional.</param>
/// <param name="Required">Whether it must be answered when visible.</param>
/// <param name="Options">Options of a choice question (2 to 30); omit for other kinds.</param>
/// <param name="ScaleMin">First value of a scale (0 or 1; default 1); only for <c>Scale</c>.</param>
/// <param name="ScaleMax">Last value of a scale (2 to 10; default 5); only for <c>Scale</c>.</param>
/// <param name="MinLabel">Label of the lowest value (e.g. "Ruim"); optional.</param>
/// <param name="MaxLabel">Label of the highest value (e.g. "Excelente"); optional.</param>
/// <param name="ShowIf">Shows the question only when an earlier scale, NPS or choice question got one of the given answers; optional.</param>
public sealed record FormQuestionDto(
    string Id,
    FeedbackQuestionType Type,
    string Text,
    string? HelpText,
    bool Required,
    IReadOnlyList<FormOptionDto>? Options,
    int? ScaleMin,
    int? ScaleMax,
    string? MinLabel,
    string? MaxLabel,
    QuestionConditionDto? ShowIf);

/// <summary>An option of a choice question.</summary>
/// <param name="Id">Identifier chosen by the builder, unique in the question.</param>
/// <param name="Label">Text shown (up to 200 characters).</param>
public sealed record FormOptionDto(string Id, string Label);

/// <summary>Condition to show a question.</summary>
/// <param name="QuestionId">An earlier scale, NPS or choice question.</param>
/// <param name="AnyOf">Answers that show the question: option ids, or numbers as text (e.g. <c>["0","1","2","3","4","5","6"]</c> for NPS detractors).</param>
public sealed record QuestionConditionDto(string QuestionId, IReadOnlyList<string> AnyOf);
