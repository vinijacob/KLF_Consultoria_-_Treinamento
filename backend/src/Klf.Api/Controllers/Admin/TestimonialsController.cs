using Klf.Api.Authorization;
using Klf.Application.DTOs.Testimonials;
using Klf.Application.Services.Testimonials;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Manages testimonials and the consent of their authors in the admin panel.</summary>
[Route($"{RoutePrefix}/admin/testimonials")]
[Tags("Admin · Testimonials")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class TestimonialsController(ITestimonialService testimonialService) : ApiControllerBase
{
    private const string GetByIdRoute = "GetTestimonialById";

    /// <summary>Lists every testimonial, published or not, in display order.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TestimonialResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TestimonialResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await testimonialService.ListAsync(cancellationToken));

    /// <summary>Returns one testimonial with its consent status.</summary>
    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [ProducesResponseType<TestimonialResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestimonialResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await testimonialService.GetByIdAsync(id, cancellationToken));

    /// <summary>Adds a testimonial. Publishing requires the consent date of the author.</summary>
    [HttpPost]
    [ProducesResponseType<TestimonialResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TestimonialResponse>> CreateAsync(CreateTestimonialRequest request, CancellationToken cancellationToken)
    {
        var testimonial = await testimonialService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(GetByIdRoute, new { id = testimonial.Id }, testimonial);
    }

    /// <summary>Replaces every editable field of a testimonial.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TestimonialResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestimonialResponse>> UpdateAsync(Guid id, UpdateTestimonialRequest request, CancellationToken cancellationToken) =>
        Ok(await testimonialService.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Records that the author withdrew the consent. The testimonial is unpublished and cannot be published again;
    /// a new consent means a new testimonial.
    /// </summary>
    [HttpPost("{id:guid}/revoke-consent")]
    [ProducesResponseType<TestimonialResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestimonialResponse>> RevokeConsentAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await testimonialService.RevokeConsentAsync(id, cancellationToken));

    /// <summary>Removes a testimonial from the site (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await testimonialService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
