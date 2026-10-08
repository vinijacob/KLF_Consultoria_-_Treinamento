using Klf.Api.Authorization;
using Klf.Api.Extensions;
using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Feedback;
using Klf.Application.Services.Feedback;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>
/// Manages feedback sessions (turmas), their QR Code and results. Admins see every session; instructors only their own
/// (others answer 404).
/// </summary>
[Route($"{RoutePrefix}/admin/feedback-sessions")]
[Tags("Admin · Feedback sessions")]
[Authorize(Policy = Policies.ManageFeedback)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class FeedbackSessionsController(IFeedbackSessionService sessionService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetFeedbackSessionById";

    /// <summary>Lists the sessions, newest opening first, with filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponse<FeedbackSessionListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<FeedbackSessionListItemResponse>>> ListAsync(
        [FromQuery] FeedbackSessionListRequest request,
        CancellationToken cancellationToken) =>
        Ok(await sessionService.ListAsync(User.ToFeedbackActor(), request, cancellationToken));

    /// <summary>Totals and NPS of the sessions opened in a period (local days). Sessions with fewer than 3 responses do not enter the NPS.</summary>
    [HttpGet("summary")]
    [ProducesResponseType<FeedbackSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeedbackSummaryResponse>> GetSummaryAsync([FromQuery] FeedbackSummaryRequest request, CancellationToken cancellationToken) =>
        Ok(await sessionService.GetSummaryAsync(User.ToFeedbackActor(), request, cancellationToken));

    /// <summary>Returns one session with its public address and frozen questions.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<FeedbackSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackSessionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await sessionService.GetByIdAsync(User.ToFeedbackActor(), id, cancellationToken));

    /// <summary>Opens a session copying the questions of a template. The signed-in user becomes its manager.</summary>
    [HttpPost]
    [ProducesResponseType<FeedbackSessionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeedbackSessionResponse>> CreateAsync(CreateFeedbackSessionRequest request, CancellationToken cancellationToken)
    {
        var session = await sessionService.CreateAsync(User.ToFeedbackActor(), request, cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = session.Id }, session);
    }

    /// <summary>Changes the name, period, response limit, client and service of a session.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<FeedbackSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackSessionResponse>> UpdateAsync(Guid id, UpdateFeedbackSessionRequest request, CancellationToken cancellationToken) =>
        Ok(await sessionService.UpdateAsync(User.ToFeedbackActor(), id, request, cancellationToken));

    /// <summary>Replaces the questions of this session only. Fails with 409 once anyone has answered.</summary>
    [HttpPut("{id:guid}/form")]
    [ProducesResponseType<FeedbackSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FeedbackSessionResponse>> ReplaceFormAsync(Guid id, ReplaceSessionFormRequest request, CancellationToken cancellationToken) =>
        Ok(await sessionService.ReplaceFormAsync(User.ToFeedbackActor(), id, request, cancellationToken));

    /// <summary>Stops accepting responses now.</summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType<FeedbackSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackSessionResponse>> CloseAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await sessionService.CloseAsync(User.ToFeedbackActor(), id, cancellationToken));

    /// <summary>Undoes a manual close; the session follows its period and limit again.</summary>
    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType<FeedbackSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackSessionResponse>> ReopenAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await sessionService.ReopenAsync(User.ToFeedbackActor(), id, cancellationToken));

    /// <summary>Removes a session from the panel (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await sessionService.DeleteAsync(User.ToFeedbackActor(), id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Aggregated results. Nothing is shown before 3 responses; a question answered by fewer than 3 people shows only its count;
    /// text answers come in random order.
    /// </summary>
    [HttpGet("{id:guid}/results")]
    [ProducesResponseType<FeedbackSessionResultsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackSessionResultsResponse>> GetResultsAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await sessionService.GetResultsAsync(User.ToFeedbackActor(), id, cancellationToken));

    /// <summary>Downloads the QR Code of the public address as a PNG.</summary>
    [HttpGet("{id:guid}/qrcode")]
    [Produces("image/png")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "image/png")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> GetQrCodeAsync(Guid id, CancellationToken cancellationToken)
    {
        var file = await sessionService.GetQrCodeAsync(User.ToFeedbackActor(), id, cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    /// <summary>Downloads the printable A4 poster (PDF) with the QR Code, the period and the anonymity notice.</summary>
    [HttpGet("{id:guid}/poster")]
    [Produces("application/pdf")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> GetPosterAsync(Guid id, CancellationToken cancellationToken)
    {
        var file = await sessionService.GetPosterAsync(User.ToFeedbackActor(), id, cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }
}
