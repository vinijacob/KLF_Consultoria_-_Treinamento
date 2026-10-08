using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Filters of a session query. Every filter is optional.</summary>
/// <param name="OwnerId">Only sessions managed by this user (instructors).</param>
/// <param name="ClientId">Only sessions of this company or store.</param>
/// <param name="ServiceId">Only sessions of this training.</param>
/// <param name="Search">Text searched in the session name.</param>
/// <param name="OpensFrom">Only sessions opening at or after this time (UTC).</param>
/// <param name="OpensBefore">Only sessions opening before this time (UTC).</param>
public sealed record FeedbackSessionFilter(
    Guid? OwnerId = null,
    Guid? ClientId = null,
    Guid? ServiceId = null,
    string? Search = null,
    DateTime? OpensFrom = null,
    DateTime? OpensBefore = null);

/// <summary>Data access for <see cref="FeedbackSession"/>. Soft-deleted sessions are never returned.</summary>
public interface IFeedbackSessionRepository
{
    /// <summary>Returns a page of sessions, newest opening first, read-only, plus the total count.</summary>
    Task<(IReadOnlyList<FeedbackSession> Items, int TotalItems)> ListAsync(
        FeedbackSessionFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>Returns every session matching the filter, newest opening first, read-only.</summary>
    Task<IReadOnlyList<FeedbackSession>> ListAllAsync(FeedbackSessionFilter filter, CancellationToken cancellationToken);

    /// <summary>Returns the session with the given id, tracked for changes, or <see langword="null"/>.</summary>
    Task<FeedbackSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns the session with the given public code, read-only, or <see langword="null"/>.</summary>
    Task<FeedbackSession?> GetByPublicCodeAsync(string publicCode, CancellationToken cancellationToken);

    /// <summary>Marks a new session to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(FeedbackSession session);

    /// <summary>Marks a session to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(FeedbackSession session);
}
