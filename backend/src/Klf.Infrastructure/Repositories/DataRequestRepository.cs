using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class DataRequestRepository(AppDbContext context) : IDataRequestRepository
{
    public async Task<(IReadOnlyList<DataRequest> Items, int Total)> ListAsync(
        DataRequestStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = context.DataRequests.AsNoTracking();

        if (status is { } wanted)
        {
            query = query.Where(request => request.Status == wanted);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(request => request.Status == DataRequestStatus.Open ? 0 : 1)
            .ThenBy(request => request.Status == DataRequestStatus.Open ? request.DueOn : DateOnly.MaxValue)
            .ThenByDescending(request => request.ReceivedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<DataRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.DataRequests.FirstOrDefaultAsync(request => request.Id == id, cancellationToken);

    public async Task<(int Open, int Overdue)> CountOpenAsync(DateOnly today, CancellationToken cancellationToken)
    {
        var open = context.DataRequests.Where(request => request.Status == DataRequestStatus.Open);

        return (await open.CountAsync(cancellationToken), await open.CountAsync(request => request.DueOn < today, cancellationToken));
    }

    public Task<bool> ProtocolExistsAsync(string protocol, CancellationToken cancellationToken) =>
        context.DataRequests.AnyAsync(request => request.Protocol == protocol, cancellationToken);

    public Task<int> DeleteClosedBeforeAsync(DateTime cutoff, CancellationToken cancellationToken) =>
        context.DataRequests
            .Where(request => request.Status != DataRequestStatus.Open && request.ClosedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

    public void Add(DataRequest request) => context.DataRequests.Add(request);
}
