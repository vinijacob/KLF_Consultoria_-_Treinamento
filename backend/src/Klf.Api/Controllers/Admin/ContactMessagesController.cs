using Klf.Api.Authorization;
using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Contact;
using Klf.Application.Services.Contact;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Inbox of the contact form. Messages hold personal data: only Admin sees them, and deleting is for good.</summary>
[Route($"{RoutePrefix}/admin/contact-messages")]
[Tags("Admin · Contact messages")]
[Authorize(Policy = Policies.ManageContactMessages)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class ContactMessagesController(IContactMessageService contactService) : ApiControllerBase
{
    /// <summary>Lists messages, newest first, filtered by status and by text in the name, e-mail or company.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponse<ContactMessageListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<ContactMessageListItemResponse>>> ListAsync([FromQuery] ContactMessageListRequest request, CancellationToken cancellationToken) =>
        Ok(await contactService.ListAsync(request, cancellationToken));

    /// <summary>How many messages nobody opened yet, for the overview.</summary>
    [HttpGet("summary")]
    [ProducesResponseType<ContactMessagesSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ContactMessagesSummaryResponse>> SummaryAsync(CancellationToken cancellationToken) =>
        Ok(await contactService.SummaryAsync(cancellationToken));

    /// <summary>Returns one message with every field.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ContactMessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactMessageResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await contactService.GetByIdAsync(id, cancellationToken));

    /// <summary>Marks a message as read, answered or unread (<c>New</c>).</summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<ContactMessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactMessageResponse>> UpdateStatusAsync(Guid id, UpdateContactMessageStatusRequest request, CancellationToken cancellationToken) =>
        Ok(await contactService.UpdateStatusAsync(id, request, cancellationToken));

    /// <summary>Deletes a message for good, personal data included (LGPD). There is no undo.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await contactService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
