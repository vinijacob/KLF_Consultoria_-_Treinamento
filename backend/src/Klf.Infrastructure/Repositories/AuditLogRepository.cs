using System.Linq.Expressions;

using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class AuditLogRepository(AppDbContext context) : IAuditLogRepository
{
    public async Task<(IReadOnlyList<AuditLog> Items, int Total)> ListAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        Guid? actorId,
        IReadOnlyList<string> actionPrefixes,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = context.AuditLogs.AsNoTracking();

        if (fromUtc is { } from)
        {
            query = query.Where(entry => entry.OccurredAt >= from);
        }

        if (toUtc is { } to)
        {
            query = query.Where(entry => entry.OccurredAt < to);
        }

        if (actorId is { } actor)
        {
            query = query.Where(entry => entry.ActorId == actor);
        }

        if (actionPrefixes.Count > 0)
        {
            query = query.Where(StartsWithAny(actionPrefixes));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(entry => entry.OccurredAt)
            .ThenByDescending(entry => entry.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<int> DeleteOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken) =>
        context.AuditLogs.Where(entry => entry.OccurredAt < cutoff).ExecuteDeleteAsync(cancellationToken);

    public void Add(AuditLog entry) => context.AuditLogs.Add(entry);

    /// <summary><c>Action LIKE 'a%' OR Action LIKE 'b%'…</c>; the prefixes come from the code (no wildcards from users).</summary>
    private static Expression<Func<AuditLog, bool>> StartsWithAny(IReadOnlyList<string> prefixes)
    {
        var entry = Expression.Parameter(typeof(AuditLog), "entry");
        var action = Expression.Property(entry, nameof(AuditLog.Action));
        Expression? body = null;

        foreach (var prefix in prefixes)
        {
            var like = Expression.Call(
                typeof(DbFunctionsExtensions),
                nameof(DbFunctionsExtensions.Like),
                Type.EmptyTypes,
                Expression.Constant(EF.Functions),
                action,
                Expression.Constant(prefix + "%"));

            body = body is null ? like : Expression.OrElse(body, like);
        }

        return Expression.Lambda<Func<AuditLog, bool>>(body!, entry);
    }
}
