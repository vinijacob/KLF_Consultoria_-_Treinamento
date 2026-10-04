using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class MediaAssetRepository(AppDbContext context) : IMediaAssetRepository
{
    public async Task<(IReadOnlyList<MediaAsset> Items, int TotalItems)> ListAsync(int skip, int take, CancellationToken cancellationToken)
    {
        var query = context.MediaAssets.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(media => media.CreatedAt)
            .ThenByDescending(media => media.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.MediaAssets.FirstOrDefaultAsync(media => media.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await context.MediaAssets
            .AsNoTracking()
            .Where(media => ids.Contains(media.Id))
            .ToListAsync(cancellationToken);

    public async Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken) =>
        await context.AlbumMedia.AnyAsync(
            item => item.MediaAssetId == id && context.Albums.Any(album => album.Id == item.AlbumId),
            cancellationToken)
        || await context.Albums.AnyAsync(album => album.CoverId == id, cancellationToken)
        || await context.Posts.AnyAsync(post => post.CoverId == id, cancellationToken)
        || await context.Services.AnyAsync(service => service.CoverId == id, cancellationToken)
        || await context.Clients.AnyAsync(client => client.LogoId == id, cancellationToken)
        || await context.Testimonials.AnyAsync(testimonial => testimonial.PhotoId == id, cancellationToken);

    public void Add(MediaAsset mediaAsset) => context.MediaAssets.Add(mediaAsset);

    public void Remove(MediaAsset mediaAsset) => context.MediaAssets.Remove(mediaAsset);
}
