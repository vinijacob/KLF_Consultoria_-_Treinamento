using FluentValidation;

using Klf.Application.DTOs.Users;
using Klf.Domain.Common;

namespace Klf.Application.Validators.Users;

internal static class UserRules
{
    public static IRuleBuilderOptions<T, string> MustBeFullName<T>(this IRuleBuilder<T, string> rule) => rule
        .Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length >= 2).WithMessage("Informe o nome da pessoa.")
        .MaximumLength(150).WithMessage("O nome pode ter no máximo 150 caracteres.");

    public static IRuleBuilderOptions<T, string> MustBePanelRole<T>(this IRuleBuilder<T, string> rule) => rule
        .Must(role => Roles.All.Contains(role)).WithMessage("Perfil inválido. Use Admin ou Editor.");
}

/// <summary>Validates <see cref="InviteUserRequest"/>.</summary>
public sealed class InviteUserRequestValidator : AbstractValidator<InviteUserRequest>
{
    /// <summary>Defines the rules.</summary>
    public InviteUserRequestValidator()
    {
        RuleFor(x => x.FullName).MustBeFullName();

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .MaximumLength(256).WithMessage("O e-mail pode ter no máximo 256 caracteres.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Role).MustBePanelRole();
    }
}

/// <summary>Validates <see cref="UpdateUserRequest"/>.</summary>
public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    /// <summary>Defines the rules.</summary>
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).MustBeFullName();
        RuleFor(x => x.Role).MustBePanelRole();
    }
}

/// <summary>Validates <see cref="AcceptInviteRequest"/>. Password strength is checked by Identity when it is saved.</summary>
public sealed class AcceptInviteRequestValidator : AbstractValidator<AcceptInviteRequest>
{
    /// <summary>Defines the rules.</summary>
    public AcceptInviteRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Link de convite incompleto. Abra o link do e-mail de novo.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("Link de convite incompleto. Abra o link do e-mail de novo.");
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("Escolha uma senha.");
    }
}
