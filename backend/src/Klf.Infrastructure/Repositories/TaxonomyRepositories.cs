using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<TaxonomyUsage<Category>>> ListAsync(DateTime? visibleAt, CancellationToken cancellationToken)
    {
        var posts = visibleAt is { } moment ? context.Posts.Where(Post.IsVisible(moment)) : context.Posts;

        var rows = await context.Categories
            .AsNoTracking()
            .Select(category => new { Category = category, Count = posts.Count(post => post.CategoryId == category.Id) })
            .Where(row => visibleAt == null || row.Count > 0)
            .OrderBy(row => row.Category.Name)
            .ToListAsync(cancellationToken);

        return [.. rows.Select(row => new TaxonomyUsage<Category>(row.Category, row.Count))];
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Categories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Categories.AnyAsync(category => category.Slug == slug && category.Id != excludingId, cancellationToken);

    public void Add(Category category) => context.Categories.Add(category);

    public void Remove(Category category) => context.Categories.Remove(category);
}

internal sealed class TagRepository(AppDbContext context) : ITagRepository
{
    public async Task<IReadOnlyList<TaxonomyUsage<Tag>>> ListAsync(DateTime? visibleAt, CancellationToken cancellationToken)
    {
        var posts = visibleAt is { } moment ? context.Posts.Where(Post.IsVisible(moment)) : context.Posts;

        var rows = await context.Tags
            .AsNoTracking()
            .Select(tag => new { Tag = tag, Count = posts.Count(post => post.Tags.Any(link => link.TagId == tag.Id)) })
            .Where(row => visibleAt == null || row.Count > 0)
            .OrderBy(row => row.Tag.Name)
            .ToListAsync(cancellationToken);

        return [.. rows.Select(row => new TaxonomyUsage<Tag>(row.Tag, row.Count))];
    }

    public Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Tags.FirstOrDefaultAsync(tag => tag.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Tag>> FindBySlugsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken) =>
        await context.Tags.Where(tag => slugs.Contains(tag.Slug)).ToListAsync(cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Tags.AnyAsync(tag => tag.Slug == slug && tag.Id != excludingId, cancellationToken);

    public void Add(Tag tag) => context.Tags.Add(tag);

    public void Remove(Tag tag) => context.Tags.Remove(tag);
}
