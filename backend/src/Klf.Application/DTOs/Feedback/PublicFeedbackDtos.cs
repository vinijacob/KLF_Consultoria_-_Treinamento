using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Feedback;

/// <summary>What the anonymous feedback page needs. The questions come only while the session is open.</summary>
/// <param name="SessionTitle">Name of the session.</param>
/// <param name="FormTitle">Title of the form.</param>
/// <param name="FormDescription">Text shown at the top of the form.</param>
/// <param name="Status"><c>Scheduled</c> (not open yet), <c>Open</c> or <c>Closed</c>.</param>
/// <param name="OpensAt">When it opens (UTC), to tell early visitors.</param>
/// <param name="AlreadyAnswered">This browser already answered this session.</param>
/// <param name="Definition">Sections and questions; only while open and not answered yet.</param>
public sealed record PublicFeedbackFormResponse(
    string SessionTitle,
    string FormTitle,
    string? FormDescription,
    FeedbackSessionStatus Status,
    DateTime OpensAt,
    bool AlreadyAnswered,
    FormDefinitionDto? Definition);

/// <summary>An anonymous response. Send only the answered questions; hidden ones (by a condition) are ignored.</summary>
/// <param name="Answers">One item per answered question.</param>
public sealed record SubmitFeedbackRequest(IReadOnlyList<FeedbackAnswerDto> Answers);

/// <summary>Answer to one question: fill the field of its kind.</summary>
/// <param name="QuestionId">The question.</param>
/// <param name="Text">For <c>ShortText</c> (up to 300 characters) and <c>LongText</c> (up to 3000).</param>
/// <param name="Number">For <c>Scale</c> and <c>Nps</c>.</param>
/// <param name="Choices">Option ids for <c>SingleChoice</c> (one) and <c>MultipleChoice</c> (one or more).</param>
public sealed record FeedbackAnswerDto(string QuestionId, string? Text, int? Number, IReadOnlyList<string>? Choices);
