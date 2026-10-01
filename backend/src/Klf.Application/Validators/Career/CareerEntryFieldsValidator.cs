using FluentValidation;

using Klf.Application.DTOs.Career;

namespace Klf.Application.Validators.Career;

/// <summary>Rules for every request that carries <see cref="ICareerEntryFields"/>.</summary>
/// <typeparam name="T">Request type.</typeparam>
public abstract class CareerEntryFieldsValidator<T> : AbstractValidator<T>
    where T : ICareerEntryFields
{
    /// <summary>Largest title length, matching the database column.</summary>
    public const int TitleMaxLength = 150;

    /// <summary>Largest institution length, matching the database column.</summary>
    public const int InstitutionMaxLength = 150;

    /// <summary>Largest description length, matching the database column.</summary>
    public const int DescriptionMaxLength = 1000;

    /// <summary>Defines the shared rules.</summary>
    protected CareerEntryFieldsValidator()
    {
        RuleFor(x => x.EntryType)
            .IsInEnum().WithMessage("Tipo inválido. Use Education, Certification ou Experience.");

        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o título.")
            .MaximumLength(TitleMaxLength).WithMessage($"O título pode ter no máximo {TitleMaxLength} caracteres.");

        RuleFor(x => x.Institution)
            .MaximumLength(InstitutionMaxLength).WithMessage($"A instituição pode ter no máximo {InstitutionMaxLength} caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(DescriptionMaxLength).WithMessage($"A descrição pode ter no máximo {DescriptionMaxLength} caracteres.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate is not null)
            .WithMessage("A data de término não pode ser anterior à data de início.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("A ordem não pode ser negativa.");
    }
}

/// <summary>Validates <see cref="CreateCareerEntryRequest"/>.</summary>
public sealed class CreateCareerEntryRequestValidator : CareerEntryFieldsValidator<CreateCareerEntryRequest>;

/// <summary>Validates <see cref="UpdateCareerEntryRequest"/>.</summary>
public sealed class UpdateCareerEntryRequestValidator : CareerEntryFieldsValidator<UpdateCareerEntryRequest>;
