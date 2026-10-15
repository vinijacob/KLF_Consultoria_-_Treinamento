namespace Klf.Application.Interfaces.Audit;

/// <summary>An action recorded explicitly (sign-in, team changes…); content changes are recorded automatically.</summary>
/// <param name="Action">One of <c>AuditActions</c>.</param>
/// <param name="TargetId">Record affected, when there is one.</param>
/// <param name="TargetLabel">Short description of the record, never personal data of visitors.</param>
/// <param name="ActorId">Who did it, when it is not the signed-in user of the request (e.g. during sign-in).</param>
/// <param name="ActorName">Name of that person.</param>
public sealed record AuditEntry(string Action, Guid? TargetId = null, string? TargetLabel = null, Guid? ActorId = null, string? ActorName = null);

/// <summary>Writes explicit entries to the audit log right away.</summary>
public interface IAuditTrail
{
    /// <summary>Saves the entry. Without <see cref="AuditEntry.ActorId"/> and <see cref="AuditEntry.ActorName"/>, the signed-in user of the request is the actor.</summary>
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken);
}
