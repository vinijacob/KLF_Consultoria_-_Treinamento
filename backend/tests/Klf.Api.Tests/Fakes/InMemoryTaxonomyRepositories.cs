using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Api.Tests.Fakes;

public sealed class InMemoryCategoryRepository(List<Post> posts) : ICategoryRepository
{
    public List<Category> Categories { get; } = [];

    public Task<IReadOnlyList<TaxonomyUsage<Category>>> ListAsync(DateTime? visibleAt, CancellationToken cancellationToken)
    {
        var counted = visibleAt is { } now ? posts.Where(Post.IsVisible(now).Compile()).ToList() : posts;

        return Task.FromResult<IReadOnlyList<TaxonomyUsage<Category>>>([.. Categories
            .Select(category => new TaxonomyUsage<Category>(category, counted.Count(post => post.CategoryId == category.Id)))
            .Where(usage => visibleAt is null || usage.PostCount > 0)
            .OrderBy(usage => usage.Item.Name, StringComparer.Ordinal)]);
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Categories.SingleOrDefault(category => category.Id == id));

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        Task.FromResult(Categories.Any(category => category.Slug == slug && category.Id != excludingId));

    public void Add(Category category) => Categories.Add(category);

    public void Remove(Category category) => Categories.Remove(category);
}

public sealed class InMemoryTagRepository(List<Post> posts) : ITagRepository
{
    public List<Tag> Tags { get; } = [];

    public Task<IReadOnlyList<TaxonomyUsage<Tag>>> ListAsync(DateTime? visibleAt, CancellationToken cancellationToken)
    {
        var counted = visibleAt is { } now ? posts.Where(Post.IsVisible(now).Compile()).ToList() : posts;

        return Task.FromResult<IReadOnlyList<TaxonomyUsage<Tag>>>([.. Tags
            .Select(tag => new TaxonomyUsage<Tag>(tag, counted.Count(post => post.Tags.Any(link => link.TagId == tag.Id))))
            .Where(usage => visibleAt is null || usage.PostCount > 0)
            .OrderBy(usage => usage.Item.Name, StringComparer.Ordinal)]);
    }

    public Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Tags.SingleOrDefault(tag => tag.Id == id));

    public Task<IReadOnlyList<Tag>> FindBySlugsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Tag>>([.. Tags.Where(tag => slugs.Contains(tag.Slug))]);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        Task.FromResult(Tags.Any(tag => tag.Slug == slug && tag.Id != excludingId));

    public void Add(Tag tag) => Tags.Add(tag);

    public void Remove(Tag tag) => Tags.Remove(tag);
}
