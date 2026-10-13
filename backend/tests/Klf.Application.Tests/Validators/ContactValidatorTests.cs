using Klf.Application.DTOs.Contact;
using Klf.Application.Validators.Contact;

namespace Klf.Application.Tests.Validators;

public sealed class ContactValidatorTests
{
    private readonly CreateContactMessageRequestValidator _validator = new();

    private static CreateContactMessageRequest Valid() =>
        new("Ana", "ana@loja.com.br", null, null, null, "Quero um orçamento para a equipe.", true, "2026-10-12");

    [Fact]
    public void Request_is_valid_with_only_the_required_fields()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Request_is_rejected_when_the_privacy_policy_was_not_acknowledged()
    {
        var result = _validator.Validate(Valid() with { AcceptsPrivacyPolicy = false });

        Assert.Contains(result.Errors, e => e.PropertyName == "AcceptsPrivacyPolicy");
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    public void Email_is_required_and_must_be_valid(string email)
    {
        Assert.Contains(_validator.Validate(Valid() with { Email = email }).Errors, e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("oi")]
    [InlineData("          oi          ")]
    public void Message_needs_at_least_ten_characters_besides_spaces(string message)
    {
        Assert.Contains(_validator.Validate(Valid() with { Message = message }).Errors, e => e.PropertyName == "Message");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12/10/2026")]
    public void Policy_version_must_be_a_date(string version)
    {
        Assert.Contains(_validator.Validate(Valid() with { PrivacyPolicyVersion = version }).Errors, e => e.PropertyName == "PrivacyPolicyVersion");
    }

    [Fact]
    public void Phone_with_letters_is_rejected()
    {
        Assert.Contains(_validator.Validate(Valid() with { Phone = "liga pra mim" }).Errors, e => e.PropertyName == "Phone");
    }
}
