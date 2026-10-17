using Klf.Application.DTOs.Common;
using Klf.Domain.Enums;

namespace Klf.Application.DTOs.DataRequests;

/// <summary>A request about personal data sent through the public site (LGPD, art. 18).</summary>
/// <param name="Name">Name of the person (up to 120 characters).</param>
/// <param name="Email">E-mail for the answer (up to 200 characters).</param>
/// <param name="Kind">What is asked.</param>
/// <param name="Details">More information (up to 2000 characters); required for correction and "other".</param>
/// <param name="Website">Trap field for robots; people leave it empty (the request is then silently discarded).</param>
public sealed record CreateDataRequestRequest(string Name, string Email, DataRequestKind Kind, string? Details, string? Website = null);

/// <summary>What the person gets after sending a request.</summary>
/// <param name="Protocol">Code to mention when talking to KLF about the request.</param>
/// <param name="DueOn">Last day for the answer (Manaus time).</param>
public sealed record DataRequestReceiptResponse(string Protocol, DateOnly DueOn);

/// <summary>Pagination and filter for the list of data requests.</summary>
public sealed record DataRequestListRequest : PagedRequest
{
    /// <summary>Only requests with this status.</summary>
    public DataRequestStatus? Status { get; init; }
}

/// <summary>A data request in the list of the panel.</summary>
/// <param name="Id">Identifier.</param>
/// <param name="Protocol">Code given to the person.</param>
/// <param name="Name">Name of the person.</param>
/// <param name="Email">E-mail of the person.</param>
/// <param name="Kind">What is asked.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="ReceivedAt">When it arrived.</param>
/// <param name="DueOn">Last day to answer.</param>
/// <param name="IsOverdue">Still open after the last day.</param>
public sealed record DataRequestListItemResponse(
    Guid Id,
    string Protocol,
    string Name,
    string Email,
    DataRequestKind Kind,
    DataRequestStatus Status,
    DateTime ReceivedAt,
    DateOnly DueOn,
    bool IsOverdue);

/// <summary>A step of the handling of a request.</summary>
/// <param name="At">When.</param>
/// <param name="Text">What was done.</param>
public sealed record DataRequestEventResponse(DateTime At, string Text);

/// <summary>A data request with everything the panel shows.</summary>
/// <param name="Id">Identifier.</param>
/// <param name="Protocol">Code given to the person.</param>
/// <param name="Name">Name of the person.</param>
/// <param name="Email">E-mail of the person.</param>
/// <param name="Kind">What is asked.</param>
/// <param name="Details">Free text from the person.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="ReceivedAt">When it arrived.</param>
/// <param name="DueOn">Last day to answer.</param>
/// <param name="IsOverdue">Still open after the last day.</param>
/// <param name="ClosedAt">When it was completed or declined.</param>
/// <param name="Resolution">What was answered, or why it was declined.</param>
/// <param name="History">Steps taken, oldest first.</param>
/// <param name="DeleteAfter">When the request itself will be deleted (only once closed).</param>
public sealed record DataRequestResponse(
    Guid Id,
    string Protocol,
    string Name,
    string Email,
    DataRequestKind Kind,
    string? Details,
    DataRequestStatus Status,
    DateTime ReceivedAt,
    DateOnly DueOn,
    bool IsOverdue,
    DateTime? ClosedAt,
    string? Resolution,
    IReadOnlyList<DataRequestEventResponse> History,
    DateTime? DeleteAfter);

/// <summary>Completes, declines or reopens a request.</summary>
/// <param name="Status">New status.</param>
/// <param name="Resolution">What was answered, or why it was declined (required to decline; up to 2000 characters).</param>
public sealed record UpdateDataRequestStatusRequest(DataRequestStatus Status, string? Resolution);

/// <summary>Counts for the menu and the overview of the panel.</summary>
/// <param name="OpenCount">Requests waiting for an answer.</param>
/// <param name="OverdueCount">Of those, how many passed the deadline.</param>
public sealed record DataRequestsSummaryResponse(int OpenCount, int OverdueCount);

/// <summary>Where to look for the data of the person (query string <c>?name=</c>).</summary>
public sealed record DataRequestFindingsRequest
{
    /// <summary>Name searched in the testimonials; defaults to the name in the request.</summary>
    public string? Name { get; init; }
}

/// <summary>A contact message sent with the e-mail of the request.</summary>
/// <param name="Id">Message id.</param>
/// <param name="ReceivedAt">When it arrived.</param>
/// <param name="Excerpt">Beginning of the message.</param>
public sealed record FoundContactMessage(Guid Id, DateTime ReceivedAt, string Excerpt);

/// <summary>A testimonial whose author name matches the request, deleted ones included.</summary>
/// <param name="Id">Testimonial id.</param>
/// <param name="AuthorName">Author.</param>
/// <param name="CompanyName">Company of the author.</param>
/// <param name="IsPublished">Shown on the site now.</param>
/// <param name="IsDeleted">Removed from the panel, but the data is still stored.</param>
/// <param name="HasPhoto">Has a photo of the person.</param>
/// <param name="HasConsentDocument">Has the signed term (PDF).</param>
public sealed record FoundTestimonial(
    Guid Id,
    string AuthorName,
    string? CompanyName,
    bool IsPublished,
    bool IsDeleted,
    bool HasPhoto,
    bool HasConsentDocument);

/// <summary>What the system holds about the person of a request.</summary>
/// <param name="SearchedName">Name used to look for testimonials.</param>
/// <param name="ContactMessages">Contact messages with the same e-mail.</param>
/// <param name="Testimonials">Testimonials with a matching author name.</param>
/// <param name="HasPanelAccount">The e-mail belongs to an account of the panel (handled in Team, not here).</param>
public sealed record DataRequestFindingsResponse(
    string SearchedName,
    IReadOnlyList<FoundContactMessage> ContactMessages,
    IReadOnlyList<FoundTestimonial> Testimonials,
    bool HasPanelAccount);
