using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Api.Tests.Fakes;

public sealed class InMemoryContactMessageRepository : IContactMessageRepository, IUnitOfWork
{
    public List<ContactMessage> Messages { get; } = [];

    public int SaveCount { get; private set; }

    public Task<(IReadOnlyList<ContactMessage> Items, int Total)> ListAsync(
        ContactMessageStatus? status,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var matching = Messages
            .Where(x => status is null || x.Status == status)
            .Where(x => search is null
                || x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (x.Company?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(x => x.ReceivedAt)
            .ToList();

        return Task.FromResult<(IReadOnlyList<ContactMessage>, int)>(([.. matching.Skip(skip).Take(take)], matching.Count));
    }

    public Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Messages.SingleOrDefault(x => x.Id == id));

    public Task<int> CountAsync(ContactMessageStatus status, CancellationToken cancellationToken) =>
        Task.FromResult(Messages.Count(x => x.Status == status));

    public Task<int> DeleteLastContactedBeforeAsync(DateTime cutoff, CancellationToken cancellationToken) =>
        Task.FromResult(Messages.RemoveAll(x => (x.AnsweredAt ?? x.ReceivedAt) < cutoff));

    public void Add(ContactMessage message) => Messages.Add(message);

    public void Remove(ContactMessage message) => Messages.Remove(message);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(1);
    }

    public Task<IReadOnlyList<ContactMessage>> ListByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ContactMessage>>([.. Messages.Where(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.ReceivedAt)]);

    public Task<int> DeleteByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(Messages.RemoveAll(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));
}
