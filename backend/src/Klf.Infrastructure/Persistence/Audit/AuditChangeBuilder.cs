using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Enums;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Persistence.Audit;

/// <summary>A tracked entity about to be saved, with the properties that changed (original and current values).</summary>
internal sealed record TrackedChange(object Entity, EntityState State, IReadOnlyDictionary<string, (object? Original, object? Current)> Modified);

/// <summary>What the audit log will record for one change.</summary>
internal sealed record AuditDraft(string Action, Guid? TargetId, string? TargetLabel);

/// <summary>
/// Turns the changes of a save into audit entries. Only content of the panel is audited: anonymous feedback responses,
/// sessions tokens, accounts (recorded explicitly) and the audit log itself never are. Testimonials and contact
/// messages carry no label, because names there are personal data.
/// </summary>
internal static class AuditChangeBuilder
{
    public static IReadOnlyList<AuditDraft> Build(IEnumerable<TrackedChange> changes)
    {
        var drafts = new List<AuditDraft>();

        foreach (var change in changes)
        {
            var draft = change.Entity is AlbumMedia item
                ? new AuditDraft("album.updated", item.AlbumId, null)
                : Describe(change);

            if (draft is not null && !drafts.Contains(draft) && !drafts.Any(existing => existing.Action == draft.Action && existing.TargetId == draft.TargetId))
            {
                drafts.Add(draft);
            }
        }

        return drafts;
    }

    private static AuditDraft? Describe(TrackedChange change)
    {
        var (area, label) = change.Entity switch
        {
            Service service => ("service", service.Title),
            Post post => ("post", post.Title),
            CareerEntry entry => ("career", entry.Title),
            Testimonial => ("testimonial", null),
            Client client => ("client", client.Name),
            MediaAsset media => ("media", media.AltText),
            Album album => ("album", album.Title),
            SiteSetting setting => ("settings", setting.Key),
            FeedbackForm form => ("feedback-form", form.Title),
            FeedbackSession session => ("feedback-session", session.Title),
            ContactMessage => ("contact", (string?)null),
            _ => (null, null),
        };

        if (area is null)
        {
            return null;
        }

        var verb = Verb(area, change);

        return verb is null ? null : new AuditDraft($"{area}.{verb}", ((Entity)change.Entity).Id, label);
    }

    private static string? Verb(string area, TrackedChange change)
    {
        switch (change.State)
        {
            case EntityState.Added:
                return area switch
                {
                    "media" => "uploaded",
                    "contact" => "received",
                    "settings" => "updated",
                    _ => "created",
                };
            case EntityState.Deleted:
                return "deleted";
            case EntityState.Modified:
                break;
            default:
                return null;
        }

        if (Changed(change, nameof(SoftDeletableEntity.DeletedAt), out _, out var deletedAt) && deletedAt is not null)
        {
            return "deleted";
        }

        return change.Entity switch
        {
            Post when Changed(change, nameof(Post.Status), out _, out var status) && status is PostStatus.Published => "published",
            Testimonial when Changed(change, nameof(Testimonial.ConsentRevokedAt), out var before, out var after) && before is null && after is not null => "consent-revoked",
            FeedbackSession when Changed(change, nameof(FeedbackSession.ClosedAt), out var before, out var after) =>
                after is not null && before is null ? "closed" : after is null ? "reopened" : "updated",
            ContactMessage => Changed(change, nameof(ContactMessage.Status), out _, out var status) && status is ContactMessageStatus.Answered ? "answered" : null,
            _ => change.Modified.Keys.Any(name => name != nameof(Entity.UpdatedAt)) ? "updated" : null,
        };
    }

    private static bool Changed(TrackedChange change, string property, out object? original, out object? current)
    {
        if (change.Modified.TryGetValue(property, out var values) && !Equals(values.Original, values.Current))
        {
            (original, current) = values;
            return true;
        }

        (original, current) = (null, null);
        return false;
    }
}
