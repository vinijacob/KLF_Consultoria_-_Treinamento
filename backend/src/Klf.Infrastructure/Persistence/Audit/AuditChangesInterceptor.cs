using Klf.Application.Interfaces.Audit;
using Klf.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Klf.Infrastructure.Persistence.Audit;

/// <summary>
/// Adds audit entries for the content changes of every save, in the same transaction as the change itself
/// (see <see cref="AuditChangeBuilder"/> for what is recorded).
/// </summary>
internal sealed class AuditChangesInterceptor(ICurrentActor actor, TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Record(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Record(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Record(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var changes = context.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && entry.Entity is not AuditLog)
            .Select(entry => new TrackedChange(
                entry.Entity,
                entry.State,
                entry.Properties.Where(property => property.IsModified)
                    .ToDictionary(property => property.Metadata.Name, property => (property.OriginalValue, property.CurrentValue))))
            .ToList();

        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var draft in AuditChangeBuilder.Build(changes))
        {
            context.Add(new AuditLog(actor.UserId, actor.Name, draft.Action, draft.TargetId, draft.TargetLabel, now));
        }
    }
}
