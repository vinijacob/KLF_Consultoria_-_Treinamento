using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Posts;
using Klf.Application.Services.Posts;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Blog posts, projects and news shown on the public site.</summary>
[Route($"{RoutePrefix}/public/posts")]
[Tags("Posts")]
public sealed class PostsController(IPostService postService) : ApiControllerBase
{
    /// <summary>Lists the published posts, newest first, with optional filters and pagination.</summary>
    /// <remarks>Drafts and posts scheduled for the future never appear here.</remarks>
    [HttpGet]
    [ProducesResponseType<PagedResponse<PostListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<PostListItemResponse>>> ListAsync([FromQuery] PostListRequest request, CancellationToken cancellationToken) =>
        Ok(await postService.ListPublicAsync(request, cancellationToken));

    /// <summary>Returns a published post by its slug (the part of the URL, e.g. <c>como-motivar-equipes</c>).</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType<PublicPostResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicPostResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        Ok(await postService.GetPublicBySlugAsync(slug, cancellationToken));
}
