using Klf.Application.DTOs.Common;

namespace Klf.Application.DTOs.Audit;

/// <summary>One entry of the audit log.</summary>
/// <param name="Id">Identifier.</param>
/// <param name="OccurredAt">When it happened (UTC).</param>
/// <param name="ActorId">Who did it; <see langword="null"/> for the system or a site visitor.</param>
/// <param name="ActorName">Name of who did it at that moment.</param>
/// <param name="Action">What happened (<c>area.verb</c>, e.g. <c>post.published</c>).</param>
/// <param name="TargetId">Record affected, when there is one.</param>
/// <param name="TargetLabel">Short description of the record, when it is not personal data.</param>
public sealed record AuditLogResponse(Guid Id, DateTime OccurredAt, Guid? ActorId, string? ActorName, string Action, Guid? TargetId, string? TargetLabel);

/// <summary>Filters of the audit log, newest first.</summary>
public sealed record AuditLogListRequest : PagedRequest
{
    /// <summary>First day (Manaus time), inclusive.</summary>
    public DateOnly? From { get; init; }

    /// <summary>Last day (Manaus time), inclusive.</summary>
    public DateOnly? To { get; init; }

    /// <summary>Only actions of this person.</summary>
    public Guid? ActorId { get; init; }

    /// <summary>Only one area: <c>content</c>, <c>contact</c>, <c>feedback</c>, <c>team</c> or <c>access</c>.</summary>
    public string? Area { get; init; }
}
