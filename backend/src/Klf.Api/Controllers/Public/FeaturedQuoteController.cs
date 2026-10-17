using Klf.Application.DTOs.FeaturedQuote;
using Klf.Application.Services.FeaturedQuote;
using Klf.Domain.Exceptions;

using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Public;

/// <summary>The quote of Kilciene shown on the home page.</summary>
[Route($"{RoutePrefix}/public/featured-quote")]
[Tags("Featured quote")]
public sealed class FeaturedQuoteController(IFeaturedQuoteService quoteService) : ApiControllerBase
{
    /// <summary>Returns the quote; 404 when there is none or it is hidden.</summary>
    [HttpGet]
    [ProducesResponseType<PublicFeaturedQuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicFeaturedQuoteResponse>> GetAsync(CancellationToken cancellationToken) =>
        Ok(await quoteService.GetPublicAsync(cancellationToken) ?? throw new NotFoundException("Nenhuma citação publicada."));
}
