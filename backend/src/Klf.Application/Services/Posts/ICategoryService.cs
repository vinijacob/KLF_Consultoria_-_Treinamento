using Klf.Application.DTOs.Posts;

namespace Klf.Application.Services.Posts;

/// <summary>Manages the categories (themes) of posts.</summary>
public interface ICategoryService
{
    /// <summary>Categories with visible posts, by name (filter of the public site).</summary>
    Task<IReadOnlyList<PublicTaxonomyItemResponse>> ListPublicAsync(CancellationToken cancellationToken);

    /// <summary>Every category, by name, with its post count (admin panel).</summary>
    Task<IReadOnlyList<TaxonomyItemResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Creates a category; the slug comes from the name.</summary>
    /// <exception cref="Domain.Exceptions.ConflictException">Another category has the same address.</exception>
    Task<TaxonomyItemResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);

    /// <summary>Renames a category; the slug follows the name.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The category does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">Another category has the same address.</exception>
    Task<TaxonomyItemResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a category; its posts are left without one.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The category does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

/// <summary>Manages the tags of posts (they are created by typing them in a post).</summary>
public interface ITagService
{
    /// <summary>Tags with visible posts, by name (public site).</summary>
    Task<IReadOnlyList<PublicTaxonomyItemResponse>> ListPublicAsync(CancellationToken cancellationToken);

    /// <summary>Every tag, by name, with its post count (admin panel and suggestions in the editor).</summary>
    Task<IReadOnlyList<TaxonomyItemResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Renames a tag; the slug follows the name.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The tag does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">Another tag has the same address.</exception>
    Task<TaxonomyItemResponse> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a tag and removes it from every post.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The tag does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
