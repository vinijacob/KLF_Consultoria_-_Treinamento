using Klf.Application.DTOs.Auth;
using Klf.Application.Validators.Auth;

namespace Klf.Application.Tests.Validators;

public sealed class TwoFactorValidatorTests
{
    private readonly TwoFactorCodeRequestValidator _validator = new();

    [Theory]
    [InlineData("123456")]
    [InlineData("123 456")]
    public void Code_is_valid_when_it_has_six_digits_with_or_without_space(string code)
    {
        Assert.True(_validator.Validate(new TwoFactorCodeRequest("token", code)).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12345a")]
    public void Code_is_rejected_when_it_is_not_six_digits(string code)
    {
        var result = _validator.Validate(new TwoFactorCodeRequest("token", code));

        Assert.Contains(result.Errors, e => e.PropertyName == "Code");
    }

    [Fact]
    public void Token_is_required_on_every_second_step_request()
    {
        Assert.False(_validator.Validate(new TwoFactorCodeRequest("", "123456")).IsValid);
        Assert.False(new TwoFactorRecoveryRequestValidator().Validate(new TwoFactorRecoveryRequest("", "AAAAA-BBBBB")).IsValid);
        Assert.False(new TwoFactorSetupRequestValidator().Validate(new TwoFactorSetupRequest("")).IsValid);
    }

    [Fact]
    public void Recovery_code_is_required_and_limited_in_size()
    {
        var validator = new TwoFactorRecoveryRequestValidator();

        Assert.False(validator.Validate(new TwoFactorRecoveryRequest("t", "")).IsValid);
        Assert.False(validator.Validate(new TwoFactorRecoveryRequest("t", new string('A', 31))).IsValid);
        Assert.True(validator.Validate(new TwoFactorRecoveryRequest("t", "AAAAA-BBBBB")).IsValid);
    }
}
