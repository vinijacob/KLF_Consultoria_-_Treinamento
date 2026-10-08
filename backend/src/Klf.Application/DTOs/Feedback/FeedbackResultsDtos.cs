using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Feedback;

/// <summary>
/// Aggregated results of a session. Nothing is shown below <paramref name="MinimumResponses"/> responses, and a question
/// answered by fewer people (e.g. a conditional one) shows only its count, so no answer can be traced back to a person.
/// Text answers come in random order.
/// </summary>
/// <param name="SessionId">Session identifier.</param>
/// <param name="Title">Name of the session.</param>
/// <param name="Status"><c>Scheduled</c>, <c>Open</c> or <c>Closed</c>.</param>
/// <param name="ResponseCount">Responses received.</param>
/// <param name="MinimumResponses">Responses needed before anything is shown (and per question).</param>
/// <param name="HasEnoughResponses">Whether the session reached <paramref name="MinimumResponses"/>.</param>
/// <param name="Nps">NPS of the main NPS question (the first one about the training); empty if there is none or not enough answers.</param>
/// <param name="Sections">Results per section; empty while there are not enough responses.</param>
public sealed record FeedbackSessionResultsResponse(
    Guid SessionId,
    string Title,
    FeedbackSessionStatus Status,
    int ResponseCount,
    int MinimumResponses,
    bool HasEnoughResponses,
    NpsResult? Nps,
    IReadOnlyList<SectionResult> Sections);

/// <summary>Results of a section.</summary>
/// <param name="Id">Section identifier.</param>
/// <param name="Title">Section title.</param>
/// <param name="Topic">Whether it is about the trainee's company or about the training.</param>
/// <param name="Questions">Results per question.</param>
public sealed record SectionResult(string Id, string Title, FeedbackTopic Topic, IReadOnlyList<QuestionResult> Questions);

/// <summary>Results of a question. Only the fields of its kind are filled.</summary>
/// <param name="Id">Question identifier.</param>
/// <param name="Text">The question.</param>
/// <param name="Type">Kind of question.</param>
/// <param name="AnswerCount">How many people answered it.</param>
/// <param name="IsHidden">Answered by too few people to show details.</param>
/// <param name="Average">Average (scale and NPS).</param>
/// <param name="Distribution">How many chose each value (scale and NPS), including values nobody chose.</param>
/// <param name="Nps">Score and groups (NPS questions).</param>
/// <param name="Options">How many chose each option (choice questions).</param>
/// <param name="Texts">Text answers in random order (text questions).</param>
public sealed record QuestionResult(
    string Id,
    string Text,
    FeedbackQuestionType Type,
    int AnswerCount,
    bool IsHidden,
    double? Average,
    IReadOnlyList<ValueCount>? Distribution,
    NpsResult? Nps,
    IReadOnlyList<OptionCount>? Options,
    IReadOnlyList<string>? Texts);

/// <summary>How many chose a value.</summary>
/// <param name="Value">The value.</param>
/// <param name="Count">How many chose it.</param>
public sealed record ValueCount(int Value, int Count);

/// <summary>How many chose an option.</summary>
/// <param name="Id">Option identifier.</param>
/// <param name="Label">Option text.</param>
/// <param name="Count">How many chose it.</param>
public sealed record OptionCount(string Id, string Label, int Count);

/// <summary>Net Promoter Score: % promoters (9–10) minus % detractors (0–6), from -100 to 100.</summary>
/// <param name="Score">The score.</param>
/// <param name="Promoters">Answers 9 or 10.</param>
/// <param name="Passives">Answers 7 or 8.</param>
/// <param name="Detractors">Answers 0 to 6.</param>
/// <param name="Total">All answers.</param>
public sealed record NpsResult(int Score, int Promoters, int Passives, int Detractors, int Total);

/// <summary>Filters of the period summary (<c>?from=2026-01-01&amp;to=2026-12-31&amp;clientId=...</c>). Dates are local days of the session opening.</summary>
public sealed record FeedbackSummaryRequest
{
    /// <summary>First day (inclusive).</summary>
    public DateOnly? From { get; init; }

    /// <summary>Last day (inclusive).</summary>
    public DateOnly? To { get; init; }

    /// <summary>Only sessions of this company or store.</summary>
    public Guid? ClientId { get; init; }

    /// <summary>Only sessions of this training.</summary>
    public Guid? ServiceId { get; init; }
}

/// <summary>Summary of many sessions. Sessions below the minimum of responses count in the totals but not in the NPS.</summary>
/// <param name="SessionCount">Sessions in the period.</param>
/// <param name="ResponseCount">Responses in those sessions.</param>
/// <param name="Nps">NPS joining the main NPS question of every session with enough responses.</param>
/// <param name="Sessions">Each session, newest first.</param>
public sealed record FeedbackSummaryResponse(int SessionCount, int ResponseCount, NpsResult? Nps, IReadOnlyList<SessionSummary> Sessions);

/// <summary>A session in the summary.</summary>
/// <param name="Id">Session identifier.</param>
/// <param name="Title">Name of the session.</param>
/// <param name="OpensAt">When it opened (UTC).</param>
/// <param name="ResponseCount">Responses received.</param>
/// <param name="Nps">NPS of its main NPS question; empty below the minimum of responses.</param>
public sealed record SessionSummary(Guid Id, string Title, DateTime OpensAt, int ResponseCount, NpsResult? Nps);
