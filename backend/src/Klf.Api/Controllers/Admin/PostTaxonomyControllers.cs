using Klf.Api.Authorization;
using Klf.Application.DTOs.Posts;
using Klf.Application.Services.Posts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Manages the categories (themes) of posts in the admin panel.</summary>
[Route($"{RoutePrefix}/admin/post-categories")]
[Tags("Admin · Posts")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class PostCategoriesController(ICategoryService categoryService) : ApiControllerBase
{
    /// <summary>Lists every category, by name, with how many posts use it (drafts included).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaxonomyItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaxonomyItemResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await categoryService.ListAsync(cancellationToken));

    /// <summary>Creates a category. The address (slug) is made from the name.</summary>
    [HttpPost]
    [ProducesResponseType<TaxonomyItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaxonomyItemResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken) =>
        Ok(await categoryService.CreateAsync(request, cancellationToken));

    /// <summary>Renames a category; the address follows the name.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TaxonomyItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaxonomyItemResponse>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken) =>
        Ok(await categoryService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Deletes a category. Its posts stay, without a category.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}

/// <summary>Manages the tags of posts in the admin panel (new tags are created by typing them in a post).</summary>
[Route($"{RoutePrefix}/admin/post-tags")]
[Tags("Admin · Posts")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class PostTagsController(ITagService tagService) : ApiControllerBase
{
    /// <summary>Lists every tag, by name, with how many posts use it (drafts included).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaxonomyItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaxonomyItemResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await tagService.ListAsync(cancellationToken));

    /// <summary>Renames a tag in every post; the address follows the name.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TaxonomyItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaxonomyItemResponse>> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken) =>
        Ok(await tagService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Deletes a tag and removes it from every post.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await tagService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
