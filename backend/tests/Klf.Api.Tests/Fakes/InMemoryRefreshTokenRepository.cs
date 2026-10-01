using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Api.Tests.Fakes;

public sealed class InMemoryRefreshTokenRepository : IRefreshTokenRepository
{
    public List<RefreshToken> Tokens { get; } = [];

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(Tokens.SingleOrDefault(t => t.TokenHash == tokenHash));

    public Task<IReadOnlyList<RefreshToken>> ListActiveByUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>([.. Tokens.Where(t => t.UserId == userId && t.IsActive(utcNow))]);

    public Task<IReadOnlyList<RefreshToken>> ListActiveByFamilyAsync(Guid familyId, DateTime utcNow, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>([.. Tokens.Where(t => t.FamilyId == familyId && t.IsActive(utcNow))]);

    public void Add(RefreshToken refreshToken) => Tokens.Add(refreshToken);
}

public sealed class NoOpUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(0);
}
