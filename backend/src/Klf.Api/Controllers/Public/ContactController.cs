using Klf.Api.Extensions;
using Klf.Application.DTOs.Contact;
using Klf.Application.Services.Contact;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Klf.Api.Controllers.Public;

/// <summary>Contact form of the public site (quote requests included).</summary>
[Route($"{RoutePrefix}/public/contact")]
[Tags("Contact")]
[EnableRateLimiting(AuthenticationExtensions.ContactRateLimitPolicy)]
public sealed class ContactController(IContactMessageService contactService) : ApiControllerBase
{
    /// <summary>
    /// Sends a message to KLF. Answers 202 with no body: the message is saved and KLF is notified by e-mail.
    /// Limited to a few messages per IP every 10 minutes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SubmitAsync(CreateContactMessageRequest request, CancellationToken cancellationToken)
    {
        await contactService.SubmitAsync(request, cancellationToken);

        return Accepted();
    }
}
