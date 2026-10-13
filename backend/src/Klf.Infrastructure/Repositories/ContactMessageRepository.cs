using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class ContactMessageRepository(AppDbContext context) : IContactMessageRepository
{
    private const string LikeEscape = "\\";

    public async Task<(IReadOnlyList<ContactMessage> Items, int Total)> ListAsync(
        ContactMessageStatus? status,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = context.ContactMessages.AsNoTracking();

        if (status is { } wanted)
        {
            query = query.Where(message => message.Status == wanted);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{EscapeLike(search.Trim())}%";

            query = query.Where(message =>
                EF.Functions.ILike(message.Name, pattern, LikeEscape)
                || EF.Functions.ILike(message.Email, pattern, LikeEscape)
                || (message.Company != null && EF.Functions.ILike(message.Company, pattern, LikeEscape)));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(message => message.ReceivedAt)
            .ThenByDescending(message => message.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.ContactMessages.FirstOrDefaultAsync(message => message.Id == id, cancellationToken);

    public Task<int> CountAsync(ContactMessageStatus status, CancellationToken cancellationToken) =>
        context.ContactMessages.CountAsync(message => message.Status == status, cancellationToken);

    public Task<int> DeleteLastContactedBeforeAsync(DateTime cutoff, CancellationToken cancellationToken) =>
        context.ContactMessages
            .Where(message => (message.AnsweredAt ?? message.ReceivedAt) < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

    public void Add(ContactMessage message) => context.ContactMessages.Add(message);

    public void Remove(ContactMessage message) => context.ContactMessages.Remove(message);

    private static string EscapeLike(string text) => text
        .Replace(LikeEscape, LikeEscape + LikeEscape, StringComparison.Ordinal)
        .Replace("%", LikeEscape + "%", StringComparison.Ordinal)
        .Replace("_", LikeEscape + "_", StringComparison.Ordinal);
}
