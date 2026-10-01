using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="RefreshToken"/>. Every method returns tracked entities.</summary>
public interface IRefreshTokenRepository
{
    /// <summary>Finds a token by its SHA-256 hash, or returns <see langword="null"/>.</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>Returns the user's tokens that are neither used, revoked nor expired at <paramref name="utcNow"/>.</summary>
    Task<IReadOnlyList<RefreshToken>> ListActiveByUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>Returns the tokens of one session (family) that are neither used, revoked nor expired at <paramref name="utcNow"/>.</summary>
    Task<IReadOnlyList<RefreshToken>> ListActiveByFamilyAsync(Guid familyId, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>Marks a new token to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(RefreshToken refreshToken);
}
