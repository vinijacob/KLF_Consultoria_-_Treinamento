using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>
/// One action done in the admin panel (RF38): who, what and when. Never holds the content itself nor personal data of
/// visitors; testimonials and contact messages are referenced by id only. Kept for <see cref="RetentionYears"/> years.
/// </summary>
public sealed class AuditLog : Entity
{
    /// <summary>How long entries are kept.</summary>
    public const int RetentionYears = 2;

    /// <summary>Records an action.</summary>
    /// <param name="actorId">Who did it; <see langword="null"/> for the system or a site visitor.</param>
    /// <param name="actorName">Name of who did it at that moment (kept even if the account changes).</param>
    /// <param name="action">What happened, as in <see cref="AuditActions"/> (e.g. <c>post.published</c>).</param>
    /// <param name="targetId">Record affected, when there is one.</param>
    /// <param name="targetLabel">Short description of the record (e.g. the post title), when it is not personal data.</param>
    /// <param name="utcNow">Current UTC time.</param>
    public AuditLog(Guid? actorId, string? actorName, string action, Guid? targetId, string? targetLabel, DateTime utcNow)
    {
        ActorId = actorId;
        ActorName = Truncate(actorName, 150);
        Action = action;
        TargetId = targetId;
        TargetLabel = Truncate(targetLabel, 200);
        OccurredAt = utcNow;
    }

    private AuditLog()
    {
        Action = string.Empty;
    }

    /// <summary>Who did it; <see langword="null"/> for the system or a site visitor.</summary>
    public Guid? ActorId { get; private set; }

    /// <summary>Name of who did it at that moment.</summary>
    public string? ActorName { get; private set; }

    /// <summary>What happened (<c>area.verb</c>, see <see cref="AuditActions"/>).</summary>
    public string Action { get; private set; }

    /// <summary>Record affected, when there is one.</summary>
    public Guid? TargetId { get; private set; }

    /// <summary>Short description of the record, when it is not personal data.</summary>
    public string? TargetLabel { get; private set; }

    /// <summary>When it happened, in UTC.</summary>
    public DateTime OccurredAt { get; private set; }

    private static string? Truncate(string? text, int length) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Length <= length ? text : text[..(length - 1)] + "…";
}
