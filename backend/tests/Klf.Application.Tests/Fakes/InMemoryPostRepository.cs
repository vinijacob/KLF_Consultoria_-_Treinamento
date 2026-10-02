using Klf.Application.Interfaces.Content;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Fakes;

internal sealed class InMemoryPostRepository : IPostRepository, IUnitOfWork
{
    public List<Post> Posts { get; } = [];

    public int SaveCount { get; private set; }

    public Task<(IReadOnlyList<Post> Items, int TotalItems)> ListAsync(PostFilter filter, int skip, int take, CancellationToken cancellationToken)
    {
        var query = Posts.AsEnumerable();

        if (filter.VisibleAt is { } now)
        {
            query = query.Where(Post.IsVisible(now).Compile());
        }

        if (filter.Type is { } type)
        {
            query = query.Where(p => p.Type == type);
        }

        if (filter.Status is { } status)
        {
            query = query.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(p => p.Title.Contains(filter.Search, StringComparison.OrdinalIgnoreCase));
        }

        var all = query.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt).ToList();

        return Task.FromResult<(IReadOnlyList<Post>, int)>(([.. all.Skip(skip).Take(take)], all.Count));
    }

    public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Posts.SingleOrDefault(p => p.Id == id));

    public Task<Post?> GetVisibleBySlugAsync(string slug, DateTime utcNow, CancellationToken cancellationToken) =>
        Task.FromResult(Posts.Where(Post.IsVisible(utcNow).Compile()).SingleOrDefault(p => p.Slug == slug));

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        Task.FromResult(Posts.Any(p => p.Slug == slug && p.Id != excludingId));

    public void Add(Post post) => Posts.Add(post);

    public void Remove(Post post) => Posts.Remove(post);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeHtmlSanitizer : IHtmlContentSanitizer
{
    public string Sanitize(string html) => html.Replace("<script>", string.Empty, StringComparison.Ordinal);
}
