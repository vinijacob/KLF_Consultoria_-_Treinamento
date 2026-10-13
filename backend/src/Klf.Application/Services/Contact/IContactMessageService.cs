using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Contact;

namespace Klf.Application.Services.Contact;

/// <summary>Contact form of the public site and the inbox of the admin panel.</summary>
public interface IContactMessageService
{
    /// <summary>
    /// Saves a message sent through the site and notifies KLF by e-mail (at the contact e-mail of the site settings, when
    /// filled). Messages caught by the bot trap are dropped without saving, and the caller cannot tell the difference.
    /// </summary>
    Task SubmitAsync(CreateContactMessageRequest request, CancellationToken cancellationToken);

    /// <summary>Lists messages, newest first.</summary>
    Task<PagedResponse<ContactMessageListItemResponse>> ListAsync(ContactMessageListRequest request, CancellationToken cancellationToken);

    /// <summary>Returns one message with every field.</summary>
    Task<ContactMessageResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Moves a message to another status (read, answered, unread).</summary>
    Task<ContactMessageResponse> UpdateStatusAsync(Guid id, UpdateContactMessageStatusRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes a message for good (LGPD: the personal data goes with it).</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Counters for the admin overview.</summary>
    Task<ContactMessagesSummaryResponse> SummaryAsync(CancellationToken cancellationToken);

    /// <summary>Deletes every message past the retention period. Returns how many were deleted.</summary>
    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken);
}
