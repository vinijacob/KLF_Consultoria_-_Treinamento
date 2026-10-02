using FluentValidation;

using Klf.Application.DTOs.Settings;

namespace Klf.Application.Validators.Settings;

/// <summary>Validates <see cref="AboutSettings"/>.</summary>
public sealed class AboutSettingsValidator : AbstractValidator<AboutSettings>
{
    /// <summary>Defines the rules.</summary>
    public AboutSettingsValidator()
    {
        RuleFor(x => x.Mission).MaximumLength(1000).WithMessage("A missão pode ter no máximo 1000 caracteres.");
        RuleFor(x => x.Vision).MaximumLength(1000).WithMessage("A visão pode ter no máximo 1000 caracteres.");
        RuleFor(x => x.History).MaximumLength(5000).WithMessage("A história pode ter no máximo 5000 caracteres.");
        RuleFor(x => x.Values).MustBeShortList("valores");
        RuleFor(x => x.Differentials).MustBeShortList("diferenciais");
    }
}

/// <summary>Validates <see cref="ContactSettings"/>.</summary>
public sealed class ContactSettingsValidator : AbstractValidator<ContactSettings>
{
    /// <summary>Defines the rules.</summary>
    public ContactSettingsValidator()
    {
        RuleFor(x => x.Whatsapp)
            .Matches(@"^\d{10,15}$").When(x => !string.IsNullOrEmpty(x.Whatsapp))
            .WithMessage("Informe o WhatsApp só com números, com código do país e DDD (ex.: 5592999999999).");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(200).WithMessage("O e-mail pode ter no máximo 200 caracteres.")
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9+()\s-]{8,20}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Informe um telefone válido (números, espaços, parênteses, hífen e +).");

        RuleFor(x => x.Address).MaximumLength(300).WithMessage("O endereço pode ter no máximo 300 caracteres.");
        RuleFor(x => x.MapUrl).MustBeHttpsUrl(500);
    }
}

/// <summary>Validates <see cref="SocialSettings"/>.</summary>
public sealed class SocialSettingsValidator : AbstractValidator<SocialSettings>
{
    /// <summary>Defines the rules.</summary>
    public SocialSettingsValidator()
    {
        RuleFor(x => x.Instagram).MustBeHttpsUrl();
        RuleFor(x => x.Linkedin).MustBeHttpsUrl();
        RuleFor(x => x.Facebook).MustBeHttpsUrl();
        RuleFor(x => x.Youtube).MustBeHttpsUrl();
    }
}

/// <summary>Validates <see cref="SeoSettings"/>.</summary>
public sealed class SeoSettingsValidator : AbstractValidator<SeoSettings>
{
    /// <summary>Defines the rules.</summary>
    public SeoSettingsValidator()
    {
        RuleFor(x => x.Title).MaximumLength(60).WithMessage("O título SEO pode ter no máximo 60 caracteres.");
        RuleFor(x => x.Description).MaximumLength(160).WithMessage("A descrição SEO pode ter no máximo 160 caracteres.");
        RuleFor(x => x.ShareImageId)
            .NotEqual(Guid.Empty).When(x => x.ShareImageId is not null)
            .WithMessage("Imagem inválida.");
    }
}
