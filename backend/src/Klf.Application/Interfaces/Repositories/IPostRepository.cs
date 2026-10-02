using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Filters for <see cref="IPostRepository.ListAsync"/>; every property is optional.</summary>
/// <param name="Type">Only posts of this kind.</param>
/// <param name="Status">Only posts with this status.</param>
/// <param name="Search">Keyword searched in the title and summary, ignoring case.</param>
/// <param name="VisibleAt">When set, only posts visible on the public site at this UTC moment (see <see cref="Post.IsVisible"/>).</param>
public sealed record PostFilter(PostType? Type = null, PostStatus? Status = null, string? Search = null, DateTime? VisibleAt = null);

/// <summary>Data access for <see cref="Post"/>. Soft-deleted posts are never returned.</summary>
public interface IPostRepository
{
    /// <summary>
    /// Returns one page of posts, newest first, plus the total number of posts matching the filter. Read-only.
    /// </summary>
    Task<(IReadOnlyList<Post> Items, int TotalItems)> ListAsync(PostFilter filter, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns the post with the given id, tracked for changes, or <see langword="null"/>.</summary>
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns the post with the given slug if it is visible on the public site at <paramref name="utcNow"/>, read-only, or <see langword="null"/>.</summary>
    Task<Post?> GetVisibleBySlugAsync(string slug, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>Whether another post already uses the slug; <paramref name="excludingId"/> skips the post being edited.</summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken);

    /// <summary>Marks a new post to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Post post);

    /// <summary>Marks a post to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(Post post);
}
