using Klf.Application.DTOs.Audit;
using Klf.Application.DTOs.Common;
using Klf.Application.Interfaces.Audit;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;
using Klf.Domain.Entities;

namespace Klf.Application.Services.Audit;

/// <summary>Reading and cleaning the audit log (Admin only).</summary>
public interface IAuditLogService
{
    /// <summary>Entries newest first, with filters.</summary>
    Task<PagedResponse<AuditLogResponse>> ListAsync(AuditLogListRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes entries past the retention period. Returns how many.</summary>
    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken);
}

internal sealed class AuditTrail(IAuditLogRepository repository, IUnitOfWork unitOfWork, ICurrentActor currentActor, TimeProvider timeProvider) : IAuditTrail
{
    public async Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken)
    {
        var explicitActor = entry.ActorId is not null || entry.ActorName is not null;

        repository.Add(new AuditLog(
            explicitActor ? entry.ActorId : currentActor.UserId,
            explicitActor ? entry.ActorName : currentActor.Name,
            entry.Action,
            entry.TargetId,
            entry.TargetLabel,
            timeProvider.GetUtcNow().UtcDateTime));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class AuditLogService(IAuditLogRepository repository, TimeProvider timeProvider) : IAuditLogService
{
    public async Task<PagedResponse<AuditLogResponse>> ListAsync(AuditLogListRequest request, CancellationToken cancellationToken)
    {
        var fromUtc = request.From is { } from ? AppTimeZone.ToUtc(from.ToDateTime(TimeOnly.MinValue)) : (DateTime?)null;
        var toUtc = request.To is { } to ? AppTimeZone.ToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue)) : (DateTime?)null;
        var prefixes = request.Area is { } area ? AuditActions.Areas[area] : [];

        var (items, total) = await repository.ListAsync(fromUtc, toUtc, request.ActorId, prefixes, request.Skip, request.PageSize, cancellationToken);

        return new PagedResponse<AuditLogResponse>(
            [.. items.Select(item => new AuditLogResponse(item.Id, item.OccurredAt, item.ActorId, item.ActorName, item.Action, item.TargetId, item.TargetLabel))],
            request.Page,
            request.PageSize,
            total);
    }

    public Task<int> PurgeExpiredAsync(CancellationToken cancellationToken) =>
        repository.DeleteOlderThanAsync(timeProvider.GetUtcNow().UtcDateTime.AddYears(-AuditLog.RetentionYears), cancellationToken);
}
