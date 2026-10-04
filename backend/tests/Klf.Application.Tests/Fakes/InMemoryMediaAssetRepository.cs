using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Fakes;

internal sealed class InMemoryMediaAssetRepository : IMediaAssetRepository, IUnitOfWork
{
    public List<MediaAsset> Media { get; } = [];

    public HashSet<Guid> Referenced { get; } = [];

    public int SaveCount { get; private set; }

    public Task<(IReadOnlyList<MediaAsset> Items, int TotalItems)> ListAsync(int skip, int take, CancellationToken cancellationToken) =>
        Task.FromResult<(IReadOnlyList<MediaAsset>, int)>(([.. Media.OrderByDescending(m => m.CreatedAt).Skip(skip).Take(take)], Media.Count));

    public Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Media.SingleOrDefault(m => m.Id == id));

    public Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<MediaAsset>>([.. Media.Where(m => ids.Contains(m.Id))]);

    public Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Referenced.Contains(id));

    public void Add(MediaAsset mediaAsset) => Media.Add(mediaAsset);

    public void Remove(MediaAsset mediaAsset) => Media.Remove(mediaAsset);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(++SaveCount);

    public MediaAsset Seed(string altText = "alt")
    {
        var media = new MediaAsset("media/2026/10/" + Guid.CreateVersion7() + ".png", "foto.png", "image/png", 10, altText);
        Media.Add(media);
        return media;
    }
}
