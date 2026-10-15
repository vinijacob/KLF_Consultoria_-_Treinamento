using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="AuditLog"/>.</summary>
public interface IAuditLogRepository
{
    /// <summary>One page of entries, newest first, read-only, and the total that match.</summary>
    /// <param name="fromUtc">Only entries at or after this moment.</param>
    /// <param name="toUtc">Only entries before this moment.</param>
    /// <param name="actorId">Only entries of this person.</param>
    /// <param name="actionPrefixes">Only actions starting with one of these (empty = every action).</param>
    /// <param name="skip">How many matching entries to skip.</param>
    /// <param name="take">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<(IReadOnlyList<AuditLog> Items, int Total)> ListAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        Guid? actorId,
        IReadOnlyList<string> actionPrefixes,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>Deletes right away every entry older than <paramref name="cutoff"/>. Returns how many.</summary>
    Task<int> DeleteOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken);

    /// <summary>Marks a new entry to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(AuditLog entry);
}
