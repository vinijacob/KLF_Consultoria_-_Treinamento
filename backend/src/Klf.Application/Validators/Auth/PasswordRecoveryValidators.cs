using FluentValidation;

using Klf.Application.DTOs.Auth;

namespace Klf.Application.Validators.Auth;

/// <summary>Validates <see cref="ForgotPasswordRequest"/>.</summary>
public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    /// <summary>Defines the rules.</summary>
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");
    }
}

/// <summary>Validates <see cref="ResetPasswordRequest"/>. The password policy itself is enforced by the identity store.</summary>
public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    /// <summary>Largest password accepted, to avoid hashing huge inputs.</summary>
    public const int PasswordMaxLength = 128;

    /// <summary>Defines the rules.</summary>
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Link inválido ou expirado. Solicite um novo.");

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe a nova senha.")
            .MaximumLength(PasswordMaxLength).WithMessage($"A senha pode ter no máximo {PasswordMaxLength} caracteres.");
    }
}
