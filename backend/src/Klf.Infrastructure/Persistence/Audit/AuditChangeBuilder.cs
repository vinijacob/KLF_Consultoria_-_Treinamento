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
        var all = changes.ToList();

        foreach (var change in all)
        {
            if (IsSideEffectOfReordering(change, all))
            {
                continue;
            }

            var draft = change.Entity switch
            {
                AlbumMedia item => new AuditDraft("album.updated", item.AlbumId, null),
                PostTag link => new AuditDraft("post.updated", link.PostId, null),
                _ when IsOnlyReordered(change) => Reordered(change),
                _ => Describe(change),
            };

            if (draft is null)
            {
                continue;
            }

            var same = drafts.FindIndex(existing => existing.Action == draft.Action && existing.TargetId == draft.TargetId);

            if (same < 0)
            {
                drafts.Add(draft);
            }
            else if (drafts[same].TargetLabel is null && draft.TargetLabel is not null)
            {
                drafts[same] = draft;
            }
        }

        return [.. drafts.Where(draft => !IsRedundantUpdate(draft, drafts))];
    }

    /// <summary>
    /// A record that only changed position, while another record of the same kind really changed in this save, was moved
    /// by <see cref="DisplayOrdering"/> (the other side of a swap, or the ones closing the gap of a deleted record).
    /// </summary>
    private static bool IsSideEffectOfReordering(TrackedChange change, List<TrackedChange> all) =>
        IsOnlyReordered(change)
        && all.Any(other => other.Entity.GetType() == change.Entity.GetType() && !IsOnlyReordered(other));

    /// <summary>
    /// A save that only moved records (a swap of positions) changes two of them, but only one was chosen; which one is
    /// unknown here, so it becomes a single "changed the order" entry for the whole list.
    /// </summary>
    private static AuditDraft? Reordered(TrackedChange change) =>
        Describe(change) is { } draft ? new AuditDraft($"{draft.Action.Split('.')[0]}.reordered", null, null) : null;

    private static bool IsOnlyReordered(TrackedChange change) =>
        change is { Entity: IOrderedEntity, State: EntityState.Modified }
        && change.Modified.ContainsKey(nameof(IOrderedEntity.DisplayOrder))
        && change.Modified.Keys.All(name => name is nameof(IOrderedEntity.DisplayOrder) or nameof(Entity.UpdatedAt));

    /// <summary>"Edited" adds nothing when the same record was also created, published or deleted in this save.</summary>
    private static bool IsRedundantUpdate(AuditDraft draft, List<AuditDraft> drafts) =>
        draft.Action.EndsWith(".updated", StringComparison.Ordinal)
        && drafts.Any(other => other.TargetId == draft.TargetId && other.Action != draft.Action
            && other.Action.Split('.')[0] == draft.Action.Split('.')[0]);

    private static AuditDraft? Describe(TrackedChange change)
    {
        var (area, label) = change.Entity switch
        {
            Service service => ("service", service.Title),
            Post post => ("post", post.Title),
            CareerEntry entry => ("career", entry.Title),
            Testimonial => ("testimonial", null),
            Client client => ("client", client.Name),
            Category category => ("post-category", category.Name),
            Tag tag => ("post-tag", tag.Name),
            MediaAsset media => ("media", media.AltText),
            Album album => ("album", album.Title),
            SiteSetting setting => ("settings", setting.Key),
            FeedbackForm form => ("feedback-form", form.Title),
            FeedbackSession session => ("feedback-session", session.Title),
            ContactMessage => ("contact", (string?)null),
            DataRequest => ("data-request", null),
            FeaturedQuote quote => ("featured-quote", quote.Author),
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
                    "data-request" => "received",
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
            DataRequest => Changed(change, nameof(DataRequest.Status), out _, out var status)
                ? status switch
                {
                    DataRequestStatus.Completed => "completed",
                    DataRequestStatus.Declined => "declined",
                    _ => "reopened",
                }
                : null,
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
