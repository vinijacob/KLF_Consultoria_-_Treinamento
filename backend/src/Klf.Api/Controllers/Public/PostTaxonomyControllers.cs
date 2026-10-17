using Klf.Application.DTOs.Posts;
using Klf.Application.Services.Posts;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Categories (themes) of the posts, for the filter of the public site.</summary>
[Route($"{RoutePrefix}/public/post-categories")]
[Tags("Posts")]
public sealed class PostCategoriesController(ICategoryService categoryService) : ApiControllerBase
{
    /// <summary>Lists the categories that have visible posts, by name, with how many.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PublicTaxonomyItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PublicTaxonomyItemResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await categoryService.ListPublicAsync(cancellationToken));
}

/// <summary>Tags of the posts, for the public site.</summary>
[Route($"{RoutePrefix}/public/post-tags")]
[Tags("Posts")]
public sealed class PostTagsController(ITagService tagService) : ApiControllerBase
{
    /// <summary>Lists the tags that have visible posts, by name, with how many.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PublicTaxonomyItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PublicTaxonomyItemResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await tagService.ListPublicAsync(cancellationToken));
}
