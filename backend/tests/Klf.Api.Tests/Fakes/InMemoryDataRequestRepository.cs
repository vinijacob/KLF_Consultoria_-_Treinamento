using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Api.Tests.Fakes;

public sealed class InMemoryDataRequestRepository : IDataRequestRepository
{
    public List<DataRequest> Requests { get; } = [];

    public Task<(IReadOnlyList<DataRequest> Items, int Total)> ListAsync(DataRequestStatus? status, int skip, int take, CancellationToken cancellationToken)
    {
        var all = Requests
            .Where(x => status is null || x.Status == status)
            .OrderBy(x => x.Status == DataRequestStatus.Open ? 0 : 1)
            .ThenBy(x => x.Status == DataRequestStatus.Open ? x.DueOn : DateOnly.MaxValue)
            .ThenByDescending(x => x.ReceivedAt)
            .ToList();

        return Task.FromResult<(IReadOnlyList<DataRequest>, int)>(([.. all.Skip(skip).Take(take)], all.Count));
    }

    public Task<DataRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Requests.SingleOrDefault(x => x.Id == id));

    public Task<(int Open, int Overdue)> CountOpenAsync(DateOnly today, CancellationToken cancellationToken)
    {
        var open = Requests.Where(x => x.Status == DataRequestStatus.Open).ToList();
        return Task.FromResult((open.Count, open.Count(x => x.DueOn < today)));
    }

    public Task<bool> ProtocolExistsAsync(string protocol, CancellationToken cancellationToken) =>
        Task.FromResult(Requests.Any(x => x.Protocol == protocol));

    public Task<int> DeleteClosedBeforeAsync(DateTime cutoff, CancellationToken cancellationToken) =>
        Task.FromResult(Requests.RemoveAll(x => x.Status != DataRequestStatus.Open && x.ClosedAt < cutoff));

    public void Add(DataRequest request) => Requests.Add(request);
}
