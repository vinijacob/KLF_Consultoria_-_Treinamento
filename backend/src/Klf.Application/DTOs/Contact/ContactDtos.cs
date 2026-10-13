using Klf.Application.DTOs.Common;
using Klf.Domain.Enums;

namespace Klf.Application.DTOs.Contact;

/// <summary>Message sent through the site's contact form.</summary>
/// <param name="Name">Sender's name (up to 120 characters).</param>
/// <param name="Email">Sender's e-mail, used to reply.</param>
/// <param name="Phone">Phone or WhatsApp; optional.</param>
/// <param name="Company">Company or store; optional.</param>
/// <param name="ServiceId">Active service the sender is interested in; optional.</param>
/// <param name="Message">The message (10 to 4000 characters).</param>
/// <param name="AcceptsPrivacyPolicy">Must be <see langword="true"/>: the sender read the privacy policy.</param>
/// <param name="PrivacyPolicyVersion">Date (<c>yyyy-MM-dd</c>) of the privacy policy shown next to the form.</param>
/// <param name="Website">Trap for bots: hidden from people, so it must stay empty. When filled, the message is silently dropped.</param>
public sealed record CreateContactMessageRequest(
    string Name,
    string Email,
    string? Phone,
    string? Company,
    Guid? ServiceId,
    string Message,
    bool AcceptsPrivacyPolicy,
    string PrivacyPolicyVersion,
    string? Website = null);

/// <summary>A contact message with every field, for the admin panel.</summary>
/// <param name="Id">Identifier.</param>
/// <param name="Name">Sender's name.</param>
/// <param name="Email">Sender's e-mail.</param>
/// <param name="Phone">Phone or WhatsApp.</param>
/// <param name="Company">Company or store.</param>
/// <param name="ServiceId">Service of interest, if it still exists.</param>
/// <param name="ServiceTitle">Title of the service when the message was sent.</param>
/// <param name="Message">The message.</param>
/// <param name="Status">Where the message stands.</param>
/// <param name="ReceivedAt">When it arrived (UTC).</param>
/// <param name="ReadAt">When it was first opened (UTC).</param>
/// <param name="AnsweredAt">When it was marked as answered (UTC).</param>
/// <param name="PrivacyPolicyVersion">Privacy policy version the sender acknowledged.</param>
/// <param name="DeleteAfter">Date (UTC) after which the message is deleted automatically.</param>
public sealed record ContactMessageResponse(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Company,
    Guid? ServiceId,
    string? ServiceTitle,
    string Message,
    ContactMessageStatus Status,
    DateTime ReceivedAt,
    DateTime? ReadAt,
    DateTime? AnsweredAt,
    string PrivacyPolicyVersion,
    DateTime DeleteAfter);

/// <summary>A contact message in the admin list.</summary>
/// <param name="Id">Identifier.</param>
/// <param name="Name">Sender's name.</param>
/// <param name="Email">Sender's e-mail.</param>
/// <param name="Company">Company or store.</param>
/// <param name="ServiceTitle">Service of interest.</param>
/// <param name="Excerpt">Beginning of the message (up to 160 characters).</param>
/// <param name="Status">Where the message stands.</param>
/// <param name="ReceivedAt">When it arrived (UTC).</param>
public sealed record ContactMessageListItemResponse(
    Guid Id,
    string Name,
    string Email,
    string? Company,
    string? ServiceTitle,
    string Excerpt,
    ContactMessageStatus Status,
    DateTime ReceivedAt);

/// <summary>Pagination and filters for the admin list of contact messages, newest first.</summary>
public sealed record ContactMessageListRequest : PagedRequest
{
    /// <summary>Only messages with this status.</summary>
    public ContactMessageStatus? Status { get; init; }

    /// <summary>Text to look for in the name, e-mail or company (case ignored).</summary>
    public string? Search { get; init; }
}

/// <summary>Moves a message to another status.</summary>
/// <param name="Status">New status (<c>New</c> means "mark as unread").</param>
public sealed record UpdateContactMessageStatusRequest(ContactMessageStatus Status);

/// <summary>Counters for the admin overview.</summary>
/// <param name="NewCount">Messages nobody opened yet.</param>
public sealed record ContactMessagesSummaryResponse(int NewCount);
