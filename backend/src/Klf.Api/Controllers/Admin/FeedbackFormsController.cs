using Klf.Api.Authorization;
using Klf.Application.DTOs.Feedback;
using Klf.Application.Services.Feedback;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Manages the feedback form templates (the free form builder).</summary>
[Route($"{RoutePrefix}/admin/feedback-forms")]
[Tags("Admin · Feedback forms")]
[Authorize(Policy = Policies.ManageFeedback)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class FeedbackFormsController(IFeedbackFormService formService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetFeedbackFormById";

    /// <summary>Lists every template ordered by title.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FeedbackFormListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FeedbackFormListItemResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await formService.ListAsync(cancellationToken));

    /// <summary>Returns one template with its sections and questions.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<FeedbackFormResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackFormResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await formService.GetByIdAsync(id, cancellationToken));

    /// <summary>Creates a template. Errors in the structure come with the path of the field, e.g. <c>Definition.Sections[0].Questions[2].Options</c>.</summary>
    [HttpPost]
    [ProducesResponseType<FeedbackFormResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeedbackFormResponse>> CreateAsync(CreateFeedbackFormRequest request, CancellationToken cancellationToken)
    {
        var form = await formService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = form.Id }, form);
    }

    /// <summary>Replaces a template. Sessions already created keep their own copy of the questions.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<FeedbackFormResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackFormResponse>> UpdateAsync(Guid id, UpdateFeedbackFormRequest request, CancellationToken cancellationToken) =>
        Ok(await formService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Removes a template (soft delete). Sessions created from it are not affected.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await formService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
