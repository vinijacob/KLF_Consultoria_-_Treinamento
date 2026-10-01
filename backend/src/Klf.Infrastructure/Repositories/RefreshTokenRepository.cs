using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> ListActiveByUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken) =>
        await context.RefreshTokens
            .Where(x => x.UserId == userId && x.UsedAt == null && x.RevokedAt == null && x.ExpiresAt > utcNow)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> ListActiveByFamilyAsync(Guid familyId, DateTime utcNow, CancellationToken cancellationToken) =>
        await context.RefreshTokens
            .Where(x => x.FamilyId == familyId && x.UsedAt == null && x.RevokedAt == null && x.ExpiresAt > utcNow)
            .ToListAsync(cancellationToken);

    public void Add(RefreshToken refreshToken) => context.RefreshTokens.Add(refreshToken);
}
