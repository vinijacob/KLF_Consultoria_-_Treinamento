using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="DataRequest"/>.</summary>
public interface IDataRequestRepository
{
    /// <summary>One page of requests: open ones first by deadline, then the others newest first. Read-only.</summary>
    Task<(IReadOnlyList<DataRequest> Items, int Total)> ListAsync(DataRequestStatus? status, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns the request, tracked for changes, or <see langword="null"/>.</summary>
    Task<DataRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>How many requests are open, and how many of them are past <paramref name="today"/>'s deadline.</summary>
    Task<(int Open, int Overdue)> CountOpenAsync(DateOnly today, CancellationToken cancellationToken);

    /// <summary>Whether a request already uses the protocol.</summary>
    Task<bool> ProtocolExistsAsync(string protocol, CancellationToken cancellationToken);

    /// <summary>Deletes, right away, the requests closed before <paramref name="cutoff"/>. Returns how many.</summary>
    Task<int> DeleteClosedBeforeAsync(DateTime cutoff, CancellationToken cancellationToken);

    /// <summary>Marks a new request to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(DataRequest request);
}
