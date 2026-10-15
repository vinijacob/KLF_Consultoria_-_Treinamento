using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Infrastructure.Persistence.Audit;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Tests.Services;

public sealed class AuditChangeBuilderTests
{
    private static readonly DateTime Now = new(2026, 10, 14, 15, 0, 0, DateTimeKind.Utc);

    private static TrackedChange Change(object entity, EntityState state, params (string Name, object? Before, object? After)[] modified) =>
        new(entity, state, modified.ToDictionary(item => item.Name, item => (item.Before, item.After)));

    private static Post Post() =>
        new(Guid.CreateVersion7(), PostType.Article, "Atendimento que encanta", "atendimento", null, "{}", "<p>x</p>", null, PostStatus.Draft, null, null, null, Now);

    [Fact]
    public void Created_updated_and_soft_deleted_content_is_named_after_the_entity()
    {
        var client = new Client("Loja Centro", null, null, 0, true);

        var drafts = AuditChangeBuilder.Build(
        [
            Change(client, EntityState.Added),
            Change(new Client("Loja Norte", null, null, 0, true), EntityState.Modified, ("Name", "Loja N", "Loja Norte")),
            Change(new Client("Loja Sul", null, null, 0, true), EntityState.Modified, ("DeletedAt", null, Now)),
        ]);

        Assert.Equal(["client.created", "client.updated", "client.deleted"], drafts.Select(draft => draft.Action));
        Assert.Equal(client.Id, drafts[0].TargetId);
        Assert.Equal("Loja Centro", drafts[0].TargetLabel);
    }

    [Fact]
    public void Publishing_a_post_is_recorded_as_published()
    {
        var drafts = AuditChangeBuilder.Build([Change(Post(), EntityState.Modified, ("Status", PostStatus.Draft, PostStatus.Published))]);

        Assert.Equal("post.published", Assert.Single(drafts).Action);
    }

    [Fact]
    public void Revoking_consent_is_recorded_without_the_name_of_the_person()
    {
        var testimonial = new Testimonial("Ana Souza", null, null, "Ótimo treinamento.", null, Now, false, true, 0);

        var draft = Assert.Single(AuditChangeBuilder.Build([Change(testimonial, EntityState.Modified, ("ConsentRevokedAt", null, Now), ("IsPublished", true, false))]));

        Assert.Equal("testimonial.consent-revoked", draft.Action);
        Assert.Null(draft.TargetLabel);
    }

    [Fact]
    public void Contact_messages_are_recorded_on_arrival_answer_and_deletion_but_not_when_only_read()
    {
        static ContactMessage Message() => new("Bruno", "bruno@loja.com.br", null, null, null, null, "Quero proposta.", "2026-10-12", Now);

        var drafts = AuditChangeBuilder.Build(
        [
            Change(Message(), EntityState.Added),
            Change(Message(), EntityState.Modified, ("Status", ContactMessageStatus.New, ContactMessageStatus.Read), ("ReadAt", null, Now)),
            Change(Message(), EntityState.Modified, ("Status", ContactMessageStatus.Read, ContactMessageStatus.Answered)),
            Change(Message(), EntityState.Deleted),
        ]);

        Assert.Equal(["contact.received", "contact.answered", "contact.deleted"], drafts.Select(draft => draft.Action));
        Assert.All(drafts, draft => Assert.Null(draft.TargetLabel));
    }

    [Fact]
    public void Anonymous_feedback_responses_are_never_recorded()
    {
        var response = new FeedbackResponse(Guid.CreateVersion7(), new DateOnly(2026, 10, 14), []);

        Assert.Empty(AuditChangeBuilder.Build([Change(response, EntityState.Added)]));
    }

    [Fact]
    public void Changing_the_items_of_an_album_counts_once_as_an_album_update()
    {
        var first = new AlbumMedia(Guid.CreateVersion7(), null, 0);
        var second = new AlbumMedia(Guid.CreateVersion7(), null, 1);

        var draft = Assert.Single(AuditChangeBuilder.Build([Change(first, EntityState.Added), Change(second, EntityState.Deleted)]));

        Assert.Equal("album.updated", draft.Action);
    }

    [Fact]
    public void Saving_with_only_the_timestamp_changed_records_nothing()
    {
        Assert.Empty(AuditChangeBuilder.Build([Change(Post(), EntityState.Modified, ("UpdatedAt", null, Now))]));
    }

    [Fact]
    public void First_save_of_a_setting_is_an_update_labelled_with_its_key()
    {
        var draft = Assert.Single(AuditChangeBuilder.Build([Change(new SiteSetting("contact", "{}"), EntityState.Added)]));

        Assert.Equal("settings.updated", draft.Action);
        Assert.Equal("contact", draft.TargetLabel);
    }
}
