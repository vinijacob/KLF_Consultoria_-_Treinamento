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
        RuleFor(x => x.PortraitIds)
            .Must(ids => ids is null || ids.Count <= AboutSettings.MaxPortraits)
            .WithMessage($"Use no máximo {AboutSettings.MaxPortraits} fotos no carrossel.")
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
            .WithMessage("A mesma foto aparece duas vezes no carrossel.")
            .Must(ids => ids is null || !ids.Contains(Guid.Empty))
            .WithMessage("Imagem inválida.");
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
        RuleFor(x => x.Links)
            .Must(links => links is null || links.Count <= SocialSettings.MaxLinks)
            .WithMessage($"Informe no máximo {SocialSettings.MaxLinks} redes sociais.")
            .Must(links => links is null || links.DistinctBy(link => link.Name.Trim().ToUpperInvariant()).Count() == links.Count)
            .WithMessage("Essa rede social já está na lista.");

        RuleForEach(x => x.Links).ChildRules(link =>
        {
            link.RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Informe o nome da rede social.")
                .Must(name => name.Trim().Length <= 40).WithMessage("O nome pode ter no máximo 40 caracteres.");

            link.RuleFor(x => x.Url)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Informe o link do perfil.")
                .MustBeHttpsUrl();
        });
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

/// <summary>Validates <see cref="LegalSettings"/>.</summary>
public sealed class LegalSettingsValidator : AbstractValidator<LegalSettings>
{
    /// <summary>Defines the rules.</summary>
    public LegalSettingsValidator()
    {
        RuleFor(x => x.CompanyName).MaximumLength(200).WithMessage("A razão social pode ter no máximo 200 caracteres.");

        RuleFor(x => x.Cnpj)
            .Must(Cnpj.IsValid).When(x => !string.IsNullOrWhiteSpace(x.Cnpj))
            .WithMessage("CNPJ inválido. Confira os números (os dois últimos são dígitos verificadores).");

        RuleFor(x => x.Address).MaximumLength(300).WithMessage("O endereço pode ter no máximo 300 caracteres.");

        RuleFor(x => x.ForumCity)
            .Matches(@"^[\p{L} .'-]{2,80}/[A-Z]{2}$").When(x => !string.IsNullOrWhiteSpace(x.ForumCity))
            .WithMessage("Informe a cidade e a sigla do estado, como em Manaus/AM.");

        RuleFor(x => x.DpoName).MaximumLength(120).WithMessage("O nome do encarregado pode ter no máximo 120 caracteres.");

        RuleFor(x => x.DpoEmail)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(200).WithMessage("O e-mail pode ter no máximo 200 caracteres.")
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.DpoEmail)).WithMessage("Informe um e-mail válido.");
    }
}
