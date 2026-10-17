using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="Tag"/>.</summary>
public interface ITagRepository
{
    /// <summary>
    /// Returns every tag by name, with its post count, read-only. With <paramref name="visibleAt"/>, counts only posts
    /// visible on the public site at that moment and leaves out tags without any.
    /// </summary>
    Task<IReadOnlyList<TaxonomyUsage<Tag>>> ListAsync(DateTime? visibleAt, CancellationToken cancellationToken);

    /// <summary>Returns the tag, tracked for changes, or <see langword="null"/>.</summary>
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns the existing tags with these slugs, tracked for changes.</summary>
    Task<IReadOnlyList<Tag>> FindBySlugsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken);

    /// <summary>Whether another tag already uses the slug; <paramref name="excludingId"/> skips the one being edited.</summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken);

    /// <summary>Marks a new tag to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Tag tag);

    /// <summary>Marks a tag to be deleted on the next save; it is removed from every post.</summary>
    void Remove(Tag tag);
}
