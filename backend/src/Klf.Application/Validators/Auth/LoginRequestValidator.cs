using FluentValidation;

using Klf.Application.DTOs.Auth;

namespace Klf.Application.Validators.Auth;

/// <summary>Ensures the login request has a well-formed e-mail and a password.</summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>Defines the login rules.</summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Informe a senha.");
    }
}
