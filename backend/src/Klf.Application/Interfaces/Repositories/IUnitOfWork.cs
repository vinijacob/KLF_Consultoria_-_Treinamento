namespace Klf.Application.Interfaces.Repositories;

/// <summary>
/// Commits every change prepared by the repositories in the current request, in a single database transaction.
/// Repositories never save on their own; services call this once, at the end of the operation.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Saves all pending changes and returns the number of affected rows.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
