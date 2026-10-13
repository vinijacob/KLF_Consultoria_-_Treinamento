using Klf.Domain.Common;
using Klf.Domain.Enums;

namespace Klf.Domain.Entities;

/// <summary>
/// A message sent through the site's contact form (quote requests included). Holds personal data, so it is deleted for
/// real (never soft deleted) and kept for at most <see cref="RetentionYears"/> years after the last contact.
/// </summary>
public sealed class ContactMessage : Entity
{
    /// <summary>How long a message is kept after the last contact (LGPD: no longer than needed).</summary>
    public const int RetentionYears = 2;

    /// <summary>Records a message received now.</summary>
    /// <param name="name">Sender's name.</param>
    /// <param name="email">Sender's e-mail, used to reply.</param>
    /// <param name="phone">Sender's phone or WhatsApp; optional.</param>
    /// <param name="company">Sender's company or store; optional.</param>
    /// <param name="serviceId">Service the sender is interested in; optional.</param>
    /// <param name="serviceTitle">Title of that service when the message was sent, kept even if the service changes.</param>
    /// <param name="text">The message itself.</param>
    /// <param name="privacyPolicyVersion">Date of the privacy policy the sender acknowledged (<c>yyyy-MM-dd</c>).</param>
    /// <param name="utcNow">Current UTC time.</param>
    public ContactMessage(
        string name,
        string email,
        string? phone,
        string? company,
        Guid? serviceId,
        string? serviceTitle,
        string text,
        string privacyPolicyVersion,
        DateTime utcNow)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Company = company;
        ServiceId = serviceId;
        ServiceTitle = serviceTitle;
        Text = text;
        PrivacyPolicyVersion = privacyPolicyVersion;
        ReceivedAt = utcNow;
        Status = ContactMessageStatus.New;
    }

    private ContactMessage()
    {
        Name = Email = Text = PrivacyPolicyVersion = string.Empty;
    }

    /// <summary>Sender's name.</summary>
    public string Name { get; private set; }

    /// <summary>Sender's e-mail.</summary>
    public string Email { get; private set; }

    /// <summary>Sender's phone or WhatsApp; optional.</summary>
    public string? Phone { get; private set; }

    /// <summary>Sender's company or store; optional.</summary>
    public string? Company { get; private set; }

    /// <summary>Service the sender is interested in; cleared if that service is deleted for good.</summary>
    public Guid? ServiceId { get; private set; }

    /// <summary>Title of the service when the message was sent.</summary>
    public string? ServiceTitle { get; private set; }

    /// <summary>The message.</summary>
    public string Text { get; private set; }

    /// <summary>Date (<c>yyyy-MM-dd</c>) of the privacy policy version the sender acknowledged before sending.</summary>
    public string PrivacyPolicyVersion { get; private set; }

    /// <summary>When the message arrived, in UTC (also the moment the privacy notice was acknowledged).</summary>
    public DateTime ReceivedAt { get; private set; }

    /// <summary>Where the message stands in the panel.</summary>
    public ContactMessageStatus Status { get; private set; }

    /// <summary>When it was first opened in the panel, in UTC.</summary>
    public DateTime? ReadAt { get; private set; }

    /// <summary>When it was marked as answered, in UTC. Counts as the last contact for the retention period.</summary>
    public DateTime? AnsweredAt { get; private set; }

    /// <summary>Moves the message to <paramref name="status"/>, keeping the dates of when it was first read and answered.</summary>
    /// <param name="status">New status. Going back to <see cref="ContactMessageStatus.New"/> means "mark as unread".</param>
    /// <param name="utcNow">Current UTC time.</param>
    public void ChangeStatus(ContactMessageStatus status, DateTime utcNow)
    {
        if (status == Status)
        {
            return;
        }

        if (status != ContactMessageStatus.New)
        {
            ReadAt ??= utcNow;
        }

        AnsweredAt = status == ContactMessageStatus.Answered ? utcNow : null;
        Status = status;
        MarkAsUpdated();
    }

    /// <summary>Whether the retention period is over: more than <see cref="RetentionYears"/> years since the last contact.</summary>
    /// <param name="utcNow">Current UTC time.</param>
    public bool IsExpired(DateTime utcNow) => (AnsweredAt ?? ReceivedAt) < RetentionCutoff(utcNow);

    /// <summary>Messages whose last contact is before this moment must be deleted.</summary>
    /// <param name="utcNow">Current UTC time.</param>
    public static DateTime RetentionCutoff(DateTime utcNow) => utcNow.AddYears(-RetentionYears);
}
