using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.DataRequests;

namespace Klf.Application.Services.DataRequests;

/// <summary>
/// Requests of data subjects about their personal data (LGPD, art. 18): received through the site, answered within
/// 15 days in the panel, where KLF also finds and deletes for real the data of the person.
/// </summary>
public interface IDataRequestService
{
    /// <summary>Records a request from the site and notifies KLF (data protection officer, or the contact e-mail).</summary>
    Task<DataRequestReceiptResponse> SubmitAsync(CreateDataRequestRequest request, CancellationToken cancellationToken);

    /// <summary>Lists the requests: open ones first, by deadline.</summary>
    Task<PagedResponse<DataRequestListItemResponse>> ListAsync(DataRequestListRequest request, CancellationToken cancellationToken);

    /// <summary>Returns one request.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The request does not exist.</exception>
    Task<DataRequestResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Counts open and overdue requests.</summary>
    Task<DataRequestsSummaryResponse> SummaryAsync(CancellationToken cancellationToken);

    /// <summary>Completes, declines (with the reason) or reopens a request.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The request does not exist.</exception>
    Task<DataRequestResponse> UpdateStatusAsync(Guid id, UpdateDataRequestStatusRequest request, CancellationToken cancellationToken);

    /// <summary>Looks for the data of the person: contact messages by e-mail, testimonials by name, panel account by e-mail.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The request does not exist.</exception>
    Task<DataRequestFindingsResponse> FindDataAsync(Guid id, DataRequestFindingsRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes for real every contact message sent with the e-mail of the request, and records it.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The request does not exist.</exception>
    Task<DataRequestResponse> EraseContactMessagesAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes for real a testimonial (even one already removed from the panel), its signed term and its photo when no
    /// other content uses it, and records it.
    /// </summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The request or the testimonial does not exist.</exception>
    Task<DataRequestResponse> EraseTestimonialAsync(Guid id, Guid testimonialId, CancellationToken cancellationToken);

    /// <summary>Deletes requests closed more than 5 years ago. Returns how many.</summary>
    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken);
}
