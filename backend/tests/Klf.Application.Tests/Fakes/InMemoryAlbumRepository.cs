using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Fakes;

internal sealed class InMemoryAlbumRepository : IAlbumRepository, IUnitOfWork
{
    public List<Album> Albums { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<Album>> ListAsync(bool onlyActive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Album>>([.. Albums.Where(a => !onlyActive || a.IsActive).OrderBy(a => a.DisplayOrder).ThenBy(a => a.Title)]);

    public Task<Album?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Albums.SingleOrDefault(a => a.Id == id));

    public Task<Album?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken) =>
        Task.FromResult(Albums.SingleOrDefault(a => a.Slug == slug && a.IsActive));

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        Task.FromResult(Albums.Any(a => a.Slug == slug && a.Id != excludingId));

    public void Add(Album album) => Albums.Add(album);

    public void Remove(Album album) => Albums.Remove(album);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(++SaveCount);
}
