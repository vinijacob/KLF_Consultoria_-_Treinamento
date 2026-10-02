using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Posts;

namespace Klf.Application.Services.Posts;

/// <summary>Manages blog posts, projects and news, for the admin panel and the public site.</summary>
public interface IPostService
{
    /// <summary>Lists the posts visible on the public site (published, or scheduled with the date already reached), newest first.</summary>
    Task<PagedResponse<PostListItemResponse>> ListPublicAsync(PostListRequest request, CancellationToken cancellationToken);

    /// <summary>Returns a post visible on the public site by its slug.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">No visible post has this slug (drafts and future posts count as not found).</exception>
    Task<PublicPostResponse> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Lists posts of any status for the admin panel, newest first.</summary>
    Task<PagedResponse<PostListItemResponse>> ListAsync(AdminPostListRequest request, CancellationToken cancellationToken);

    /// <summary>Returns one post with every field.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The post does not exist.</exception>
    Task<PostResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Creates a post authored by <paramref name="authorId"/>.</summary>
    /// <exception cref="Domain.Exceptions.ConflictException">Another post already uses the slug.</exception>
    Task<PostResponse> CreateAsync(Guid authorId, CreatePostRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces every editable field of a post.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The post does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">Another post already uses the slug.</exception>
    Task<PostResponse> UpdateAsync(Guid id, UpdatePostRequest request, CancellationToken cancellationToken);

    /// <summary>Soft-deletes a post; it disappears from the site but stays in the database.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The post does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
