using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Fakes;

internal sealed class InMemoryRefreshTokenRepository : IRefreshTokenRepository, IUnitOfWork
{
    public List<RefreshToken> Tokens { get; } = [];

    public int SaveCount { get; private set; }

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(Tokens.SingleOrDefault(t => t.TokenHash == tokenHash));

    public Task<IReadOnlyList<RefreshToken>> ListActiveByUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>([.. Tokens.Where(t => t.UserId == userId && t.IsActive(utcNow))]);

    public Task<IReadOnlyList<RefreshToken>> ListActiveByFamilyAsync(Guid familyId, DateTime utcNow, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>([.. Tokens.Where(t => t.FamilyId == familyId && t.IsActive(utcNow))]);

    public void Add(RefreshToken refreshToken) => Tokens.Add(refreshToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}
