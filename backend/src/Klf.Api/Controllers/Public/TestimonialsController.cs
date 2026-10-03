using Klf.Application.DTOs.Testimonials;
using Klf.Application.Services.Testimonials;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>Testimonials shown on the public site.</summary>
[Route($"{RoutePrefix}/public/testimonials")]
[Tags("Testimonials")]
public sealed class TestimonialsController(ITestimonialService testimonialService) : ApiControllerBase
{
    /// <summary>Lists the testimonials that are published with valid consent, in display order.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PublicTestimonialResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PublicTestimonialResponse>>> ListAsync(CancellationToken cancellationToken) =>
        Ok(await testimonialService.ListPublicAsync(cancellationToken));
}
