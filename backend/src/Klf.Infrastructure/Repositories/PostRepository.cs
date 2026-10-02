using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class PostRepository(AppDbContext context) : IPostRepository
{
    private const string LikeEscape = "\\";

    public async Task<(IReadOnlyList<Post> Items, int TotalItems)> ListAsync(
        PostFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = context.Posts.AsNoTracking();

        if (filter.VisibleAt is { } visibleAt)
        {
            query = query.Where(Post.IsVisible(visibleAt));
        }

        if (filter.Type is { } type)
        {
            query = query.Where(post => post.Type == type);
        }

        if (filter.Status is { } status)
        {
            query = query.Where(post => post.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{EscapeLike(filter.Search)}%";

            query = query.Where(post =>
                EF.Functions.ILike(post.Title, pattern, LikeEscape)
                || (post.Summary != null && EF.Functions.ILike(post.Summary, pattern, LikeEscape)));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(post => post.PublishedAt ?? post.CreatedAt)
            .ThenByDescending(post => post.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Posts.FirstOrDefaultAsync(post => post.Id == id, cancellationToken);

    public Task<Post?> GetVisibleBySlugAsync(string slug, DateTime utcNow, CancellationToken cancellationToken) =>
        context.Posts
            .AsNoTracking()
            .Where(Post.IsVisible(utcNow))
            .FirstOrDefaultAsync(post => post.Slug == slug, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Posts.AnyAsync(post => post.Slug == slug && post.Id != excludingId, cancellationToken);

    public void Add(Post post) => context.Posts.Add(post);

    public void Remove(Post post) => context.Posts.Remove(post);

    private static string EscapeLike(string text) => text
        .Replace(LikeEscape, LikeEscape + LikeEscape, StringComparison.Ordinal)
        .Replace("%", LikeEscape + "%", StringComparison.Ordinal)
        .Replace("_", LikeEscape + "_", StringComparison.Ordinal);
}
