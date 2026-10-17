using Klf.Api.Authorization;
using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.DataRequests;
using Klf.Application.Services.DataRequests;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Klf.Api.Controllers.Admin;

/// <summary>Requests of data subjects (LGPD): answer within 15 days, find and delete for real the data of the person.</summary>
[Route($"{RoutePrefix}/admin/data-requests")]
[Tags("Admin · Data requests")]
[Authorize(Policy = Policies.ManageDataRequests)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class DataRequestsController(IDataRequestService dataRequestService) : ApiControllerBase
{
    /// <summary>Lists the requests: open ones first, by deadline.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponse<DataRequestListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<DataRequestListItemResponse>>> ListAsync([FromQuery] DataRequestListRequest request, CancellationToken cancellationToken) =>
        Ok(await dataRequestService.ListAsync(request, cancellationToken));

    /// <summary>How many requests are open and how many passed the deadline (menu and overview).</summary>
    [HttpGet("summary")]
    [ProducesResponseType<DataRequestsSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DataRequestsSummaryResponse>> SummaryAsync(CancellationToken cancellationToken) =>
        Ok(await dataRequestService.SummaryAsync(cancellationToken));

    /// <summary>Returns one request with its history.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<DataRequestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataRequestResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await dataRequestService.GetByIdAsync(id, cancellationToken));

    /// <summary>Completes, declines (with the reason) or reopens a request.</summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<DataRequestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataRequestResponse>> UpdateStatusAsync(Guid id, UpdateDataRequestStatusRequest request, CancellationToken cancellationToken) =>
        Ok(await dataRequestService.UpdateStatusAsync(id, request, cancellationToken));

    /// <summary>Looks for the data of the person: contact messages (e-mail), testimonials (name, <c>?name=</c> to try another) and panel account.</summary>
    [HttpGet("{id:guid}/findings")]
    [ProducesResponseType<DataRequestFindingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataRequestFindingsResponse>> FindDataAsync(Guid id, [FromQuery] DataRequestFindingsRequest request, CancellationToken cancellationToken) =>
        Ok(await dataRequestService.FindDataAsync(id, request, cancellationToken));

    /// <summary>Deletes for real every contact message sent with the e-mail of the request.</summary>
    [HttpPost("{id:guid}/erase-contact-messages")]
    [ProducesResponseType<DataRequestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataRequestResponse>> EraseContactMessagesAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await dataRequestService.EraseContactMessagesAsync(id, cancellationToken));

    /// <summary>Deletes for real a testimonial of the person, its signed term and its photo (when no other content uses it).</summary>
    [HttpPost("{id:guid}/erase-testimonials/{testimonialId:guid}")]
    [ProducesResponseType<DataRequestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataRequestResponse>> EraseTestimonialAsync(Guid id, Guid testimonialId, CancellationToken cancellationToken) =>
        Ok(await dataRequestService.EraseTestimonialAsync(id, testimonialId, cancellationToken));
}
