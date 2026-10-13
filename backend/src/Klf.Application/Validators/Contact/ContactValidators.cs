using FluentValidation;

using Klf.Application.DTOs.Contact;
using Klf.Application.Validators.Common;

namespace Klf.Application.Validators.Contact;

/// <summary>Validates <see cref="CreateContactMessageRequest"/>.</summary>
public sealed class CreateContactMessageRequestValidator : AbstractValidator<CreateContactMessageRequest>
{
    /// <summary>Largest name length, matching the database column.</summary>
    public const int NameMaxLength = 120;

    /// <summary>Largest e-mail length, matching the database column.</summary>
    public const int EmailMaxLength = 200;

    /// <summary>Largest company length, matching the database column.</summary>
    public const int CompanyMaxLength = 150;

    /// <summary>Shortest message.</summary>
    public const int MessageMinLength = 10;

    /// <summary>Largest message, matching the database column.</summary>
    public const int MessageMaxLength = 4000;

    /// <summary>Defines the rules.</summary>
    public CreateContactMessageRequestValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe seu nome.")
            .MaximumLength(NameMaxLength).WithMessage($"O nome pode ter no máximo {NameMaxLength} caracteres.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe seu e-mail para podermos responder.")
            .MaximumLength(EmailMaxLength).WithMessage($"O e-mail pode ter no máximo {EmailMaxLength} caracteres.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9+()\s-]{8,20}$").When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Informe um telefone válido, com DDD (números, espaços, parênteses, hífen e +).");

        RuleFor(x => x.Company).MaximumLength(CompanyMaxLength).WithMessage($"A empresa pode ter no máximo {CompanyMaxLength} caracteres.");

        RuleFor(x => x.ServiceId)
            .NotEqual(Guid.Empty).When(x => x.ServiceId is not null)
            .WithMessage("Serviço inválido.");

        RuleFor(x => x.Message)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Escreva sua mensagem.")
            .Must(text => text.Trim().Length >= MessageMinLength).WithMessage($"Conte um pouco mais: pelo menos {MessageMinLength} caracteres.")
            .MaximumLength(MessageMaxLength).WithMessage($"A mensagem pode ter no máximo {MessageMaxLength} caracteres.");

        RuleFor(x => x.AcceptsPrivacyPolicy)
            .Equal(true).WithMessage("Para enviar, confirme que leu a política de privacidade.");

        RuleFor(x => x.PrivacyPolicyVersion)
            .Must(version => DateOnly.TryParseExact(version, "yyyy-MM-dd", out _))
            .WithMessage("Versão da política de privacidade inválida. Recarregue a página e tente de novo.");

        RuleFor(x => x.Website).MaximumLength(300);
    }
}

/// <summary>Validates <see cref="ContactMessageListRequest"/>.</summary>
public sealed class ContactMessageListRequestValidator : AbstractValidator<ContactMessageListRequest>
{
    /// <summary>Defines the rules.</summary>
    public ContactMessageListRequestValidator()
    {
        Include(new PagedRequestValidator());

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status is not null)
            .WithMessage("Status inválido. Use New, Read ou Answered.");

        RuleFor(x => x.Search).MaximumLength(100).WithMessage("A busca pode ter no máximo 100 caracteres.");
    }
}

/// <summary>Validates <see cref="UpdateContactMessageStatusRequest"/>.</summary>
public sealed class UpdateContactMessageStatusRequestValidator : AbstractValidator<UpdateContactMessageStatusRequest>
{
    /// <summary>Defines the rules.</summary>
    public UpdateContactMessageStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status inválido. Use New, Read ou Answered.");
    }
}
