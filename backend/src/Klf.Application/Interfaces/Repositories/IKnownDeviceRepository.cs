using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="KnownDevice"/>.</summary>
public interface IKnownDeviceRepository
{
    /// <summary>Returns the device of the account with the given token hash, tracked for changes, or <see langword="null"/>.</summary>
    Task<KnownDevice?> FindAsync(Guid userId, string tokenHash, CancellationToken cancellationToken);

    /// <summary>Whether the account already has any known device.</summary>
    Task<bool> AnyAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Marks a new device to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(KnownDevice device);

    /// <summary>Deletes, right away, the devices last seen before <paramref name="cutoff"/>. Returns how many were deleted.</summary>
    Task<int> DeleteLastSeenBeforeAsync(DateTime cutoff, CancellationToken cancellationToken);
}
