using Klf.Application.DTOs.Testimonials;
using Klf.Application.Services.Testimonials;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Testimonials shown on the public site.</summary>
[Route($"{RoutePrefix}/public/testimonials")]
[Tags("Testimonials")]
public sealed class TestimonialsController(ITestimonialService testimonialService) : ApiControllerBase
{
    /// <summary>
    /// Lists the testimonials that are published with valid consent, in display order; with <c>?latest=5</c>, only the
    /// five most recently added.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PublicTestimonialResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PublicTestimonialResponse>>> ListAsync(
        [FromQuery] PublicTestimonialListRequest request,
        CancellationToken cancellationToken) =>
        Ok(await testimonialService.ListPublicAsync(request, cancellationToken));
}
