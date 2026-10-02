using Klf.Api.Authorization;
using Klf.Api.Extensions;
using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Posts;
using Klf.Application.Services.Posts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Manages blog posts, projects and news in the admin panel.</summary>
[Route($"{RoutePrefix}/admin/posts")]
[Tags("Admin · Posts")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class PostsController(IPostService postService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetPostById";

    /// <summary>Lists posts of any status (drafts included), newest first, with optional filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponse<PostListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<PostListItemResponse>>> ListAsync([FromQuery] AdminPostListRequest request, CancellationToken cancellationToken) =>
        Ok(await postService.ListAsync(request, cancellationToken));

    /// <summary>Returns one post with every field.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<PostResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await postService.GetByIdAsync(id, cancellationToken));

    /// <summary>Creates a post. The author is the signed-in user.</summary>
    [HttpPost]
    [ProducesResponseType<PostResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PostResponse>> CreateAsync(CreatePostRequest request, CancellationToken cancellationToken)
    {
        var post = await postService.CreateAsync(User.GetUserId(), request, cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = post.Id }, post);
    }

    /// <summary>Replaces every editable field of a post.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<PostResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PostResponse>> UpdateAsync(Guid id, UpdatePostRequest request, CancellationToken cancellationToken) =>
        Ok(await postService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Removes a post from the site (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await postService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
