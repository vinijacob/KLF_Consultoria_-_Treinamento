using Klf.Application.DTOs.Auth;
using Klf.Application.Validators.Auth;

namespace Klf.Application.Tests.Validators;

public sealed class PasswordRecoveryValidatorTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("nao-e-email", false)]
    [InlineData("a@klf.com.br", true)]
    public void Forgot_requires_a_valid_email(string email, bool expected)
    {
        Assert.Equal(expected, new ForgotPasswordRequestValidator().Validate(new ForgotPasswordRequest(email)).IsValid);
    }

    [Fact]
    public void Reset_requires_email_token_and_password_and_limits_password_size()
    {
        var validator = new ResetPasswordRequestValidator();

        Assert.True(validator.Validate(new ResetPasswordRequest("a@klf.com.br", "tok", "Nova@Senha1234")).IsValid);
        Assert.False(validator.Validate(new ResetPasswordRequest("", "tok", "x")).IsValid);
        Assert.False(validator.Validate(new ResetPasswordRequest("a@klf.com.br", "", "x")).IsValid);
        Assert.False(validator.Validate(new ResetPasswordRequest("a@klf.com.br", "tok", "")).IsValid);
        Assert.False(validator.Validate(new ResetPasswordRequest("a@klf.com.br", "tok", new string('a', 129))).IsValid);
    }
}
