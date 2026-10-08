using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class FeedbackFormRepository(AppDbContext context) : IFeedbackFormRepository
{
    public async Task<IReadOnlyList<FeedbackForm>> ListAsync(CancellationToken cancellationToken) =>
        await context.FeedbackForms.AsNoTracking().OrderBy(form => form.Title).ToListAsync(cancellationToken);

    public Task<FeedbackForm?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.FeedbackForms.FirstOrDefaultAsync(form => form.Id == id, cancellationToken);

    public void Add(FeedbackForm form) => context.FeedbackForms.Add(form);

    public void Remove(FeedbackForm form) => context.FeedbackForms.Remove(form);
}

internal sealed class FeedbackSessionRepository(AppDbContext context) : IFeedbackSessionRepository
{
    private const string LikeEscape = "\\";

    public async Task<(IReadOnlyList<FeedbackSession> Items, int TotalItems)> ListAsync(
        FeedbackSessionFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = Filter(filter);
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(session => session.OpensAt)
            .ThenByDescending(session => session.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<FeedbackSession>> ListAllAsync(FeedbackSessionFilter filter, CancellationToken cancellationToken) =>
        await Filter(filter)
            .OrderByDescending(session => session.OpensAt)
            .ThenByDescending(session => session.Id)
            .ToListAsync(cancellationToken);

    public Task<FeedbackSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.FeedbackSessions.FirstOrDefaultAsync(session => session.Id == id, cancellationToken);

    public Task<FeedbackSession?> GetByPublicCodeAsync(string publicCode, CancellationToken cancellationToken) =>
        context.FeedbackSessions.AsNoTracking().FirstOrDefaultAsync(session => session.PublicCode == publicCode, cancellationToken);

    public void Add(FeedbackSession session) => context.FeedbackSessions.Add(session);

    public void Remove(FeedbackSession session) => context.FeedbackSessions.Remove(session);

    private static string EscapeLike(string text) =>
        text.Replace(LikeEscape, LikeEscape + LikeEscape, StringComparison.Ordinal)
            .Replace("%", LikeEscape + "%", StringComparison.Ordinal)
            .Replace("_", LikeEscape + "_", StringComparison.Ordinal);

    private IQueryable<FeedbackSession> Filter(FeedbackSessionFilter filter)
    {
        var query = context.FeedbackSessions.AsNoTracking();

        if (filter.OwnerId is { } ownerId)
        {
            query = query.Where(session => session.OwnerId == ownerId);
        }

        if (filter.ClientId is { } clientId)
        {
            query = query.Where(session => session.ClientId == clientId);
        }

        if (filter.ServiceId is { } serviceId)
        {
            query = query.Where(session => session.ServiceId == serviceId);
        }

        if (filter.OpensFrom is { } from)
        {
            query = query.Where(session => session.OpensAt >= from);
        }

        if (filter.OpensBefore is { } before)
        {
            query = query.Where(session => session.OpensAt < before);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{EscapeLike(filter.Search)}%";
            query = query.Where(session => EF.Functions.ILike(session.Title, pattern, LikeEscape));
        }

        return query;
    }
}

internal sealed class FeedbackResponseRepository(AppDbContext context) : IFeedbackResponseRepository
{
    public Task<int> CountBySessionAsync(Guid sessionId, CancellationToken cancellationToken) =>
        context.FeedbackResponses.CountAsync(response => response.SessionId == sessionId, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, int>> CountBySessionsAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken) =>
        await context.FeedbackResponses
            .Where(response => sessionIds.Contains(response.SessionId))
            .GroupBy(response => response.SessionId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.Key, row => row.Count, cancellationToken);

    public async Task<IReadOnlyList<FeedbackResponse>> ListBySessionsAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken) =>
        await context.FeedbackResponses
            .AsNoTracking()
            .Where(response => sessionIds.Contains(response.SessionId))
            .ToListAsync(cancellationToken);

    public void Add(FeedbackResponse response) => context.FeedbackResponses.Add(response);
}
