using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Tests.Entities;

public sealed class TestimonialTests
{
    private static readonly DateTime Now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Signed = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Testimonial_is_created_published_when_consent_was_given()
    {
        var testimonial = Create(consentGivenAt: Signed, isPublished: true);

        Assert.True(testimonial.IsPublished);
        Assert.Equal(Signed, testimonial.ConsentGivenAt);
        Assert.Null(testimonial.ConsentRevokedAt);
    }

    [Fact]
    public void Draft_is_allowed_when_consent_is_not_given_yet()
    {
        var testimonial = Create(consentGivenAt: null, isPublished: false);

        Assert.False(testimonial.IsPublished);
    }

    [Fact]
    public void Creation_throws_when_publishing_without_consent()
    {
        var error = Assert.Throws<ValidationException>(() => Create(consentGivenAt: null, isPublished: true));

        Assert.Contains("IsPublished", error.Errors.Keys);
    }

    [Fact]
    public void Creation_throws_when_photo_is_used_without_image_consent()
    {
        var error = Assert.Throws<ValidationException>(() =>
            new Testimonial("Ana", null, null, "Ótimo", Guid.CreateVersion7(), Signed, consentCoversImage: false, isPublished: false, 0));

        Assert.Contains("PhotoId", error.Errors.Keys);
    }

    [Fact]
    public void Revoke_unpublishes_and_keeps_first_date_when_called_twice()
    {
        var testimonial = Create(Signed, isPublished: true);

        testimonial.RevokeConsent(Now);
        testimonial.RevokeConsent(Now.AddDays(5));

        Assert.False(testimonial.IsPublished);
        Assert.Equal(Now, testimonial.ConsentRevokedAt);
    }

    [Fact]
    public void Update_throws_when_publishing_after_consent_was_revoked()
    {
        var testimonial = Create(Signed, isPublished: true);
        testimonial.RevokeConsent(Now);

        Assert.Throws<ValidationException>(() =>
            testimonial.Update("Ana", null, null, "Ótimo", null, Signed, false, isPublished: true, 0));
        Assert.False(testimonial.IsPublished);
    }

    [Fact]
    public void Failed_update_changes_nothing_when_a_rule_is_broken()
    {
        var testimonial = Create(Signed, isPublished: true);

        Assert.Throws<ValidationException>(() =>
            testimonial.Update("Outro", null, null, "Texto novo", null, consentGivenAt: null, false, isPublished: true, 9));

        Assert.Equal("Ana", testimonial.AuthorName);
        Assert.Equal(Signed, testimonial.ConsentGivenAt);
        Assert.Equal(0, testimonial.DisplayOrder);
    }

    [Fact]
    public void Visibility_rule_requires_published_consent_and_no_revocation()
    {
        var isVisible = Testimonial.IsVisible.Compile();
        var visible = Create(Signed, isPublished: true);
        var draft = Create(Signed, isPublished: false);
        var revoked = Create(Signed, isPublished: true);
        revoked.RevokeConsent(Now);

        Assert.True(isVisible(visible));
        Assert.False(isVisible(draft));
        Assert.False(isVisible(revoked));
    }

    private static Testimonial Create(DateTime? consentGivenAt, bool isPublished) =>
        new("Ana", "Gerente", "Loja X", "Ótimo treinamento", null, consentGivenAt, false, isPublished, 0);
}
