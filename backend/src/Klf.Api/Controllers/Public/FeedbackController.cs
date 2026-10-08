using Klf.Api.Extensions;
using Klf.Application.DTOs.Feedback;
using Klf.Application.Services.Feedback;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Klf.Api.Controllers.Public;

/// <summary>
/// Anonymous feedback: the page opened from the QR Code. Nothing about the respondent is stored or logged
/// (no IP, user agent, exact time or account). Call with <c>credentials: "include"</c> so the "already answered" cookie travels.
/// </summary>
[Route($"{RoutePrefix}/public/feedback")]
[Tags("Feedback")]
[EnableRateLimiting(AuthenticationExtensions.FeedbackRateLimitPolicy)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class FeedbackController(IPublicFeedbackService feedbackService, FeedbackCookie cookie) : ApiControllerBase
{
    /// <summary>Returns the session status and, while it is open and this browser has not answered, the questions.</summary>
    [HttpGet("{code}")]
    [ProducesResponseType<PublicFeedbackFormResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PublicFeedbackFormResponse>> GetFormAsync(string code, CancellationToken cancellationToken) =>
        Ok(await feedbackService.GetFormAsync(code, cookie.HasAnswered(Request, code), cancellationToken));

    /// <summary>Sends an anonymous response. One per browser per session; answers to questions hidden by a condition are ignored.</summary>
    [HttpPost("{code}/responses")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SubmitAsync(string code, SubmitFeedbackRequest request, CancellationToken cancellationToken)
    {
        var submission = await feedbackService.SubmitAsync(code, request, cookie.HasAnswered(Request, code), cancellationToken);
        cookie.MarkAnswered(Response, code, submission.RememberUntil);

        return NoContent();
    }
}
