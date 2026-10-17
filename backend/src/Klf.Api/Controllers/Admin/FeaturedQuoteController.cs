using Klf.Api.Authorization;
using Klf.Application.DTOs.FeaturedQuote;
using Klf.Application.Services.FeaturedQuote;
using Klf.Domain.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Edits the quote of Kilciene on the home page (there is only one).</summary>
[Route($"{RoutePrefix}/admin/featured-quote")]
[Tags("Admin · Featured quote")]
[Authorize(Policy = Policies.ManageContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class FeaturedQuoteController(IFeaturedQuoteService quoteService) : ApiControllerBase
{
    /// <summary>Returns the quote; 404 while none was saved.</summary>
    [HttpGet]
    [ProducesResponseType<FeaturedQuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeaturedQuoteResponse>> GetAsync(CancellationToken cancellationToken) =>
        Ok(await quoteService.GetAsync(cancellationToken) ?? throw new NotFoundException("A citação ainda não foi preenchida."));

    /// <summary>Creates the quote or replaces it.</summary>
    [HttpPut]
    [ProducesResponseType<FeaturedQuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeaturedQuoteResponse>> SaveAsync(SaveFeaturedQuoteRequest request, CancellationToken cancellationToken) =>
        Ok(await quoteService.SaveAsync(request, cancellationToken));
}
