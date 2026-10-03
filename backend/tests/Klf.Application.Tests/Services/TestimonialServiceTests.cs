using Klf.Application.DTOs.Testimonials;
using Klf.Application.Services.Testimonials;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class TestimonialServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 2, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryTestimonialRepository _repository = new();
    private readonly TestimonialService _service;

    public TestimonialServiceTests()
    {
        _service = new TestimonialService(_repository, _repository, new MutableTimeProvider(Now));
    }

    [Fact]
    public async Task Create_saves_published_testimonial_with_trimmed_text_when_consent_was_given()
    {
        var request = Request() with { AuthorName = "  Ana  ", CompanyName = " " };

        var response = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Testimonials);
        Assert.Equal("Ana", saved.AuthorName);
        Assert.Null(saved.CompanyName);
        Assert.Equal(DateTimeKind.Utc, saved.ConsentGivenAt!.Value.Kind);
        Assert.NotNull(response.ConsentGivenAt);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Create_throws_validation_when_consent_date_is_in_the_future()
    {
        var request = Request() with { ConsentGivenAt = Now.AddDays(1) };

        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(request, TestContext.Current.CancellationToken));

        Assert.Contains("ConsentGivenAt", error.Errors.Keys);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Public_list_hides_drafts_and_revoked_and_drops_photo_without_image_consent()
    {
        var photo = Guid.CreateVersion7();
        var signed = Now.UtcDateTime.AddDays(-10);
        _repository.Testimonials.Add(new Testimonial("Visível", null, null, "ok", photo, signed, true, true, 0));
        _repository.Testimonials.Add(new Testimonial("Rascunho", null, null, "ok", null, signed, false, false, 1));
        var revoked = new Testimonial("Revogado", null, null, "ok", null, signed, false, true, 2);
        revoked.RevokeConsent(Now.UtcDateTime);
        _repository.Testimonials.Add(revoked);

        var list = await _service.ListPublicAsync(TestContext.Current.CancellationToken);

        var item = Assert.Single(list);
        Assert.Equal("Visível", item.AuthorName);
        Assert.Equal(photo, item.PhotoId);
    }

    [Fact]
    public async Task Revoke_unpublishes_testimonial_and_records_the_current_time()
    {
        var testimonial = new Testimonial("Ana", null, null, "ok", null, Now.UtcDateTime.AddDays(-1), false, true, 0);
        _repository.Testimonials.Add(testimonial);

        var response = await _service.RevokeConsentAsync(testimonial.Id, TestContext.Current.CancellationToken);

        Assert.False(response.IsPublished);
        Assert.Equal(Now, response.ConsentRevokedAt);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Update_throws_validation_when_publishing_after_revocation()
    {
        var testimonial = new Testimonial("Ana", null, null, "ok", null, Now.UtcDateTime.AddDays(-1), false, true, 0);
        testimonial.RevokeConsent(Now.UtcDateTime);
        _repository.Testimonials.Add(testimonial);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpdateAsync(testimonial.Id, new UpdateTestimonialRequest("Ana", null, null, "ok", null, Now.AddDays(-1), false, true, 0), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Get_revoke_and_delete_throw_not_found_when_testimonial_does_not_exist()
    {
        var id = Guid.CreateVersion7();
        var token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(id, token));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.RevokeConsentAsync(id, token));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(id, token));
    }

    private static CreateTestimonialRequest Request() =>
        new("Ana", "Gerente", "Loja X", "Ótimo", null, Now.AddDays(-1), false, true, 0);
}
