using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Api.Tests.Fakes;

public sealed class InMemoryKnownDeviceRepository : IKnownDeviceRepository
{
    public List<KnownDevice> Devices { get; } = [];

    public Task<KnownDevice?> FindAsync(Guid userId, string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(Devices.SingleOrDefault(x => x.UserId == userId && x.TokenHash == tokenHash));

    public Task<bool> AnyAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(Devices.Any(x => x.UserId == userId));

    public void Add(KnownDevice device) => Devices.Add(device);

    public Task<int> DeleteLastSeenBeforeAsync(DateTime cutoff, CancellationToken cancellationToken) =>
        Task.FromResult(Devices.RemoveAll(x => x.LastSeenAt < cutoff));
}
