using Klf.Application.DTOs.Contact;
using Klf.Domain.Entities;

namespace Klf.Application.Mappings;

internal static class ContactMappings
{
    private const int ExcerptLength = 160;

    public static ContactMessageResponse ToResponse(this ContactMessage message) => new(
        message.Id,
        message.Name,
        message.Email,
        message.Phone,
        message.Company,
        message.ServiceId,
        message.ServiceTitle,
        message.Text,
        message.Status,
        message.ReceivedAt,
        message.ReadAt,
        message.AnsweredAt,
        message.PrivacyPolicyVersion,
        (message.AnsweredAt ?? message.ReceivedAt).AddYears(ContactMessage.RetentionYears));

    public static ContactMessageListItemResponse ToListItem(this ContactMessage message) => new(
        message.Id,
        message.Name,
        message.Email,
        message.Company,
        message.ServiceTitle,
        Excerpt(message.Text),
        message.Status,
        message.ReceivedAt);

    private static string Excerpt(string text)
    {
        var flat = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return flat.Length <= ExcerptLength ? flat : $"{flat[..(ExcerptLength - 1)].TrimEnd()}…";
    }
}
