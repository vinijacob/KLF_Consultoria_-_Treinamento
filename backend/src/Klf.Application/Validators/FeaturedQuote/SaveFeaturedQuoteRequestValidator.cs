using FluentValidation;

using Klf.Application.DTOs.FeaturedQuote;

namespace Klf.Application.Validators.FeaturedQuote;

/// <summary>Validates <see cref="SaveFeaturedQuoteRequest"/>.</summary>
public sealed class SaveFeaturedQuoteRequestValidator : AbstractValidator<SaveFeaturedQuoteRequest>
{
    /// <summary>Largest quote, matching the database column.</summary>
    public const int TextMaxLength = 400;

    /// <summary>Largest author name, matching the database column.</summary>
    public const int AuthorMaxLength = 120;

    /// <summary>Largest source, matching the database column.</summary>
    public const int SourceMaxLength = 150;

    /// <summary>Defines the rules.</summary>
    public SaveFeaturedQuoteRequestValidator()
    {
        RuleFor(x => x.Text)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Escreva a citação.")
            .Must(text => text.Trim().Length <= TextMaxLength).WithMessage($"A citação pode ter no máximo {TextMaxLength} caracteres.");

        RuleFor(x => x.Author)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe quem disse.")
            .MaximumLength(AuthorMaxLength).WithMessage($"O autor pode ter no máximo {AuthorMaxLength} caracteres.");

        RuleFor(x => x.Source).MaximumLength(SourceMaxLength).WithMessage($"A origem pode ter no máximo {SourceMaxLength} caracteres.");
    }
}
