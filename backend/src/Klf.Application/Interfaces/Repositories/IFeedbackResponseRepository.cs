using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for anonymous <see cref="FeedbackResponse"/> rows. They are never updated nor listed one by one in the panel.</summary>
public interface IFeedbackResponseRepository
{
    /// <summary>Number of responses of a session.</summary>
    Task<int> CountBySessionAsync(Guid sessionId, CancellationToken cancellationToken);

    /// <summary>Number of responses of each session; sessions without responses are missing from the result.</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountBySessionsAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken);

    /// <summary>Every response of the given sessions, read-only, in no particular order.</summary>
    Task<IReadOnlyList<FeedbackResponse>> ListBySessionsAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken);

    /// <summary>Marks a new response to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(FeedbackResponse response);
}
