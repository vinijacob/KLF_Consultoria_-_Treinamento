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
        var query = context.Posts.AsNoTracking().Include(post => post.Category).AsQueryable();

        if (filter.VisibleAt is { } visibleAt)
        {
            query = query.Where(Post.IsVisible(visibleAt));
        }

        if (filter.Type is { } type)
        {
            query = query.Where(post => post.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            query = query.Where(post => post.Category != null && post.Category.Slug == filter.CategorySlug);
        }

        if (!string.IsNullOrWhiteSpace(filter.TagSlug))
        {
            query = query.Where(post => post.Tags.Any(link => link.Tag.Slug == filter.TagSlug));
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
        WithTaxonomy(context.Posts).FirstOrDefaultAsync(post => post.Id == id, cancellationToken);

    public Task<Post?> GetVisibleBySlugAsync(string slug, DateTime utcNow, CancellationToken cancellationToken) =>
        WithTaxonomy(context.Posts)
            .AsNoTracking()
            .Where(Post.IsVisible(utcNow))
            .FirstOrDefaultAsync(post => post.Slug == slug, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Posts.AnyAsync(post => post.Slug == slug && post.Id != excludingId, cancellationToken);

    public void Add(Post post) => context.Posts.Add(post);

    public void Remove(Post post) => context.Posts.Remove(post);

    private static IQueryable<Post> WithTaxonomy(IQueryable<Post> posts) =>
        posts
            .Include(post => post.Category)
            .Include(post => post.Tags)
            .ThenInclude(link => link.Tag)
            .AsSplitQuery();

    private static string EscapeLike(string text) => text
        .Replace(LikeEscape, LikeEscape + LikeEscape, StringComparison.Ordinal)
        .Replace("%", LikeEscape + "%", StringComparison.Ordinal)
        .Replace("_", LikeEscape + "_", StringComparison.Ordinal);
}
