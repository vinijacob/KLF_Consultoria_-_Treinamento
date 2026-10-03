using FluentValidation;

using Klf.Application.DTOs.Testimonials;

namespace Klf.Application.Validators.Testimonials;

/// <summary>Rules for every request that carries <see cref="ITestimonialFields"/>.</summary>
/// <typeparam name="T">Request type.</typeparam>
public abstract class TestimonialFieldsValidator<T> : AbstractValidator<T>
    where T : ITestimonialFields
{
    /// <summary>Largest author name length, matching the database column.</summary>
    public const int AuthorNameMaxLength = 120;

    /// <summary>Largest author role length, matching the database column.</summary>
    public const int AuthorRoleMaxLength = 120;

    /// <summary>Largest company name length, matching the database column.</summary>
    public const int CompanyNameMaxLength = 200;

    /// <summary>Largest testimonial length, matching the database column.</summary>
    public const int QuoteMaxLength = 1000;

    /// <summary>Defines the shared rules.</summary>
    protected TestimonialFieldsValidator()
    {
        RuleFor(x => x.AuthorName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o nome do autor.")
            .MaximumLength(AuthorNameMaxLength).WithMessage($"O nome do autor pode ter no máximo {AuthorNameMaxLength} caracteres.");

        RuleFor(x => x.AuthorRole)
            .MaximumLength(AuthorRoleMaxLength).WithMessage($"O cargo pode ter no máximo {AuthorRoleMaxLength} caracteres.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(CompanyNameMaxLength).WithMessage($"O nome da empresa pode ter no máximo {CompanyNameMaxLength} caracteres.");

        RuleFor(x => x.Quote)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o texto do depoimento.")
            .MaximumLength(QuoteMaxLength).WithMessage($"O depoimento pode ter no máximo {QuoteMaxLength} caracteres.");

        RuleFor(x => x.PhotoId)
            .NotEqual(Guid.Empty).When(x => x.PhotoId is not null)
            .WithMessage("Foto inválida.");

        RuleFor(x => x.ConsentCoversImage)
            .Equal(true).When(x => x.PhotoId is not null)
            .WithMessage("Para usar a foto, o consentimento precisa cobrir o uso de imagem.");

        RuleFor(x => x.ConsentGivenAt)
            .NotNull().When(x => x.IsPublished)
            .WithMessage("Só é possível publicar o depoimento depois de registrar o consentimento do autor.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("A ordem não pode ser negativa.");
    }
}

/// <summary>Validates <see cref="CreateTestimonialRequest"/>.</summary>
public sealed class CreateTestimonialRequestValidator : TestimonialFieldsValidator<CreateTestimonialRequest>;

/// <summary>Validates <see cref="UpdateTestimonialRequest"/>.</summary>
public sealed class UpdateTestimonialRequestValidator : TestimonialFieldsValidator<UpdateTestimonialRequest>;
