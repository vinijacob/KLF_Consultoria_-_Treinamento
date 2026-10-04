using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class AlbumRepository(AppDbContext context) : IAlbumRepository
{
    public async Task<IReadOnlyList<Album>> ListAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        var query = context.Albums.AsNoTracking().Include(album => album.Items).AsQueryable();

        if (onlyActive)
        {
            query = query.Where(album => album.IsActive);
        }

        return await query
            .OrderBy(album => album.DisplayOrder)
            .ThenBy(album => album.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<Album?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Albums.Include(album => album.Items).FirstOrDefaultAsync(album => album.Id == id, cancellationToken);

    public Task<Album?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken) =>
        context.Albums
            .AsNoTracking()
            .Include(album => album.Items)
            .FirstOrDefaultAsync(album => album.Slug == slug && album.IsActive, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Albums.AnyAsync(album => album.Slug == slug && album.Id != excludingId, cancellationToken);

    public void Add(Album album) => context.Albums.Add(album);

    public void Remove(Album album) => context.Albums.Remove(album);
}
