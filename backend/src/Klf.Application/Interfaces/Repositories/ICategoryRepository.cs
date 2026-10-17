using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>A category or tag with how many posts use it.</summary>
/// <typeparam name="T"><see cref="Category"/> or <see cref="Tag"/>.</typeparam>
/// <param name="Item">The category or tag.</param>
/// <param name="PostCount">How many posts use it.</param>
public sealed record TaxonomyUsage<T>(T Item, int PostCount);

/// <summary>Data access for <see cref="Category"/>.</summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Returns every category by name, with its post count, read-only. With <paramref name="visibleAt"/>, counts only posts
    /// visible on the public site at that moment and leaves out categories without any.
    /// </summary>
    Task<IReadOnlyList<TaxonomyUsage<Category>>> ListAsync(DateTime? visibleAt, CancellationToken cancellationToken);

    /// <summary>Returns the category, tracked for changes, or <see langword="null"/>.</summary>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Whether another category already uses the slug; <paramref name="excludingId"/> skips the one being edited.</summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken);

    /// <summary>Marks a new category to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Category category);

    /// <summary>Marks a category to be deleted on the next save; its posts are left without a category.</summary>
    void Remove(Category category);
}
