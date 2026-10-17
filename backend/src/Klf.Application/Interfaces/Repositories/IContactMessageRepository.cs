using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="ContactMessage"/>.</summary>
public interface IContactMessageRepository
{
    /// <summary>Returns one page of messages, newest first, read-only, and the total that match the filters.</summary>
    /// <param name="status">Only this status, when given.</param>
    /// <param name="search">Text to find in the name, e-mail or company, case ignored; optional.</param>
    /// <param name="skip">How many matching messages to skip.</param>
    /// <param name="take">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<(IReadOnlyList<ContactMessage> Items, int Total)> ListAsync(
        ContactMessageStatus? status,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>Returns the message with the given id, tracked for changes, or <see langword="null"/>.</summary>
    Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>How many messages have the given status.</summary>
    Task<int> CountAsync(ContactMessageStatus status, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes for good, right away, every message whose last contact (answer or arrival) is before <paramref name="cutoff"/>.
    /// Returns how many were deleted.
    /// </summary>
    Task<int> DeleteLastContactedBeforeAsync(DateTime cutoff, CancellationToken cancellationToken);

    /// <summary>Marks a new message to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(ContactMessage message);

    /// <summary>Messages sent with this e-mail (case ignored), newest first, read-only.</summary>
    Task<IReadOnlyList<ContactMessage>> ListByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Deletes, right away, every message sent with this e-mail (case ignored). Returns how many.</summary>
    Task<int> DeleteByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Marks a message to be deleted for good on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(ContactMessage message);
}
