using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class KnownDeviceRepository(AppDbContext context) : IKnownDeviceRepository
{
    public Task<KnownDevice?> FindAsync(Guid userId, string tokenHash, CancellationToken cancellationToken) =>
        context.KnownDevices.FirstOrDefaultAsync(x => x.UserId == userId && x.TokenHash == tokenHash, cancellationToken);

    public Task<bool> AnyAsync(Guid userId, CancellationToken cancellationToken) =>
        context.KnownDevices.AnyAsync(x => x.UserId == userId, cancellationToken);

    public void Add(KnownDevice device) => context.KnownDevices.Add(device);

    public Task<int> DeleteLastSeenBeforeAsync(DateTime cutoff, CancellationToken cancellationToken) =>
        context.KnownDevices.Where(x => x.LastSeenAt < cutoff).ExecuteDeleteAsync(cancellationToken);
}
