using Klf.Application.DTOs.Clients;
using Klf.Application.DTOs.Testimonials;
using Klf.Application.Validators.Clients;
using Klf.Application.Validators.Testimonials;

namespace Klf.Application.Tests.Validators;

public sealed class ClientAndTestimonialValidatorTests
{
    private readonly CreateClientRequestValidator _clientValidator = new();
    private readonly CreateTestimonialRequestValidator _testimonialValidator = new();

    [Fact]
    public void Client_is_valid_when_only_name_is_filled()
    {
        Assert.True(_clientValidator.Validate(new CreateClientRequest("Loja X", null, null, 0, true)).IsValid);
    }

    [Theory]
    [InlineData("http://loja.com.br")]
    [InlineData("javascript:alert(1)")]
    [InlineData("loja.com.br")]
    public void Client_link_is_rejected_when_it_is_not_https(string url)
    {
        var result = _clientValidator.Validate(new CreateClientRequest("Loja X", url, null, 0, true));

        Assert.Contains(result.Errors, e => e.PropertyName == "WebsiteUrl");
    }

    [Fact]
    public void Client_name_is_required_and_order_cannot_be_negative()
    {
        var result = _clientValidator.Validate(new CreateClientRequest(" ", null, null, -1, true));

        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.PropertyName == "DisplayOrder");
    }

    [Fact]
    public void Testimonial_is_valid_when_published_with_consent()
    {
        Assert.True(_testimonialValidator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Testimonial_publish_is_rejected_when_consent_is_missing()
    {
        var result = _testimonialValidator.Validate(Valid() with { ConsentGivenAt = null });

        Assert.Contains(result.Errors, e => e.PropertyName == "ConsentGivenAt");
    }

    [Fact]
    public void Testimonial_photo_is_rejected_when_consent_does_not_cover_image()
    {
        var result = _testimonialValidator.Validate(Valid() with { PhotoId = Guid.CreateVersion7(), ConsentCoversImage = false });

        Assert.Contains(result.Errors, e => e.PropertyName == "ConsentCoversImage");
    }

    [Fact]
    public void Testimonial_text_is_rejected_when_empty_or_too_long()
    {
        var empty = _testimonialValidator.Validate(Valid() with { Quote = "" });
        var tooLong = _testimonialValidator.Validate(Valid() with { Quote = new string('a', 1001) });

        Assert.Contains(empty.Errors, e => e.PropertyName == "Quote");
        Assert.Contains(tooLong.Errors, e => e.PropertyName == "Quote");
    }

    private static CreateTestimonialRequest Valid() =>
        new("Ana", "Gerente", "Loja X", "Ótimo", null, DateTimeOffset.UtcNow.AddDays(-1), false, true, 0);
}
