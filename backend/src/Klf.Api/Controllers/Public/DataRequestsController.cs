using Klf.Api.Extensions;
using Klf.Application.DTOs.DataRequests;
using Klf.Application.Services.DataRequests;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Klf.Api.Controllers.Public;

/// <summary>Requests of data subjects about their personal data (LGPD, art. 18), sent through the site.</summary>
[Route($"{RoutePrefix}/public/data-requests")]
[Tags("Data requests")]
[EnableRateLimiting(AuthenticationExtensions.ContactRateLimitPolicy)]
public sealed class DataRequestsController(IDataRequestService dataRequestService) : ApiControllerBase
{
    /// <summary>
    /// Records a request (access, correction, deletion, withdrawal of consent) and notifies KLF. Returns the protocol
    /// and the last day for the answer (15 days). Shares the rate limit of the contact form.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<DataRequestReceiptResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DataRequestReceiptResponse>> SubmitAsync(CreateDataRequestRequest request, CancellationToken cancellationToken) =>
        Ok(await dataRequestService.SubmitAsync(request, cancellationToken));
}
