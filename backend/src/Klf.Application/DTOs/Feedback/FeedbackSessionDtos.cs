using Klf.Application.DTOs.Common;
using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Feedback;

/// <summary>Scheduling fields shared by the create and update requests of a session.</summary>
public interface IFeedbackSessionFields
{
    /// <summary>Name of the session.</summary>
    string Title { get; }

    /// <summary>When responses start being accepted.</summary>
    DateTimeOffset OpensAt { get; }

    /// <summary>When responses stop being accepted.</summary>
    DateTimeOffset ClosesAt { get; }

    /// <summary>Response limit.</summary>
    int? MaxResponses { get; }

    /// <summary>Company or store trained.</summary>
    Guid? ClientId { get; }

    /// <summary>Training given.</summary>
    Guid? ServiceId { get; }
}

/// <summary>Data to open a session (turma) from a form template. The questions are copied, so later template edits do not affect it.</summary>
/// <param name="FormId">Template to copy.</param>
/// <param name="Title">Name of the session, shown to respondents too, e.g. "Loja Centro — Atendimento (manhã)" (up to 200 characters).</param>
/// <param name="OpensAt">When responses start being accepted (ISO 8601 with offset, e.g. <c>2026-10-07T08:00:00-04:00</c>).</param>
/// <param name="ClosesAt">When responses stop being accepted; after <c>OpensAt</c> and at most 90 days later.</param>
/// <param name="MaxResponses">Stops accepting after this many responses (1 to 5000); optional.</param>
/// <param name="ClientId">Company or store trained; optional.</param>
/// <param name="ServiceId">Training given; optional.</param>
public sealed record CreateFeedbackSessionRequest(
    Guid FormId,
    string Title,
    DateTimeOffset OpensAt,
    DateTimeOffset ClosesAt,
    int? MaxResponses,
    Guid? ClientId,
    Guid? ServiceId) : IFeedbackSessionFields;

/// <summary>Data to change the scheduling of a session. The questions change with the form endpoint.</summary>
/// <param name="Title">Name of the session (up to 200 characters).</param>
/// <param name="OpensAt">When responses start being accepted.</param>
/// <param name="ClosesAt">When responses stop being accepted; after <c>OpensAt</c> and at most 90 days later.</param>
/// <param name="MaxResponses">Response limit (1 to 5000); optional.</param>
/// <param name="ClientId">Company or store trained; optional.</param>
/// <param name="ServiceId">Training given; optional.</param>
public sealed record UpdateFeedbackSessionRequest(
    string Title,
    DateTimeOffset OpensAt,
    DateTimeOffset ClosesAt,
    int? MaxResponses,
    Guid? ClientId,
    Guid? ServiceId) : IFeedbackSessionFields;

/// <summary>New questions for one session (only while it has no responses).</summary>
/// <param name="FormTitle">Title respondents see (up to 200 characters).</param>
/// <param name="FormDescription">Text shown at the top of the form (up to 1000 characters); optional.</param>
/// <param name="Definition">Sections and questions.</param>
public sealed record ReplaceSessionFormRequest(string FormTitle, string? FormDescription, FormDefinitionDto Definition);

/// <summary>Pagination and filters of the session list (<c>?clientId=...&amp;search=manha&amp;page=2</c>).</summary>
public sealed record FeedbackSessionListRequest : PagedRequest
{
    /// <summary>Only sessions of this company or store.</summary>
    public Guid? ClientId { get; init; }

    /// <summary>Only sessions of this training.</summary>
    public Guid? ServiceId { get; init; }

    /// <summary>Text searched in the session name.</summary>
    public string? Search { get; init; }
}

/// <summary>A session with its frozen questions, for the admin panel.</summary>
/// <param name="Id">Session identifier.</param>
/// <param name="Title">Name of the session.</param>
/// <param name="PublicCode">Code of the public address.</param>
/// <param name="PublicUrl">Address respondents open (the same encoded in the QR Code).</param>
/// <param name="Status"><c>Scheduled</c>, <c>Open</c> or <c>Closed</c>.</param>
/// <param name="ResponseCount">Responses received.</param>
/// <param name="FormId">Template the questions came from.</param>
/// <param name="FormTitle">Title respondents see.</param>
/// <param name="FormDescription">Text shown at the top of the form.</param>
/// <param name="Definition">Frozen questions.</param>
/// <param name="OpensAt">When responses start being accepted (UTC).</param>
/// <param name="ClosesAt">When responses stop being accepted (UTC).</param>
/// <param name="MaxResponses">Response limit.</param>
/// <param name="ClosedAt">When it was closed by hand (UTC).</param>
/// <param name="ClientId">Company or store trained.</param>
/// <param name="ServiceId">Training given.</param>
/// <param name="OwnerId">User who manages it.</param>
public sealed record FeedbackSessionResponse(
    Guid Id,
    string Title,
    string PublicCode,
    string PublicUrl,
    FeedbackSessionStatus Status,
    int ResponseCount,
    Guid? FormId,
    string FormTitle,
    string? FormDescription,
    FormDefinitionDto Definition,
    DateTime OpensAt,
    DateTime ClosesAt,
    int? MaxResponses,
    DateTime? ClosedAt,
    Guid? ClientId,
    Guid? ServiceId,
    Guid OwnerId);

/// <summary>A session in a list.</summary>
/// <param name="Id">Session identifier.</param>
/// <param name="Title">Name of the session.</param>
/// <param name="PublicCode">Code of the public address.</param>
/// <param name="Status"><c>Scheduled</c>, <c>Open</c> or <c>Closed</c>.</param>
/// <param name="ResponseCount">Responses received.</param>
/// <param name="OpensAt">When responses start being accepted (UTC).</param>
/// <param name="ClosesAt">When responses stop being accepted (UTC).</param>
/// <param name="MaxResponses">Response limit.</param>
/// <param name="ClientId">Company or store trained.</param>
/// <param name="ServiceId">Training given.</param>
public sealed record FeedbackSessionListItemResponse(
    Guid Id,
    string Title,
    string PublicCode,
    FeedbackSessionStatus Status,
    int ResponseCount,
    DateTime OpensAt,
    DateTime ClosesAt,
    int? MaxResponses,
    Guid? ClientId,
    Guid? ServiceId);
