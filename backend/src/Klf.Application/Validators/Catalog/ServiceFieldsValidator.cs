using FluentValidation;

using Klf.Application.DTOs.Catalog;

namespace Klf.Application.Validators.Catalog;

/// <summary>Rules for every request that carries <see cref="IServiceFields"/>.</summary>
/// <typeparam name="T">Request type.</typeparam>
public abstract class ServiceFieldsValidator<T> : AbstractValidator<T>
    where T : IServiceFields
{
    /// <summary>Largest title length, matching the database column.</summary>
    public const int TitleMaxLength = 200;

    /// <summary>Largest slug length, matching the database column.</summary>
    public const int SlugMaxLength = 200;

    /// <summary>Largest summary length, matching the database column.</summary>
    public const int SummaryMaxLength = 500;

    /// <summary>Largest audience length, matching the database column.</summary>
    public const int AudienceMaxLength = 200;

    /// <summary>Largest detail text size.</summary>
    public const int ContentMaxLength = 200_000;

    /// <summary>Largest workload accepted, in hours.</summary>
    public const int WorkloadMaxHours = 2000;

    /// <summary>Defines the shared rules.</summary>
    protected ServiceFieldsValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o título.")
            .MaximumLength(TitleMaxLength).WithMessage($"O título pode ter no máximo {TitleMaxLength} caracteres.");

        RuleFor(x => x.Slug)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o slug.")
            .MaximumLength(SlugMaxLength).WithMessage($"O slug pode ter no máximo {SlugMaxLength} caracteres.")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("O slug deve conter apenas letras minúsculas, números e hífens.");

        RuleFor(x => x.Summary)
            .MaximumLength(SummaryMaxLength).WithMessage($"O resumo pode ter no máximo {SummaryMaxLength} caracteres.");

        RuleFor(x => x.ContentHtml)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o conteúdo da página do serviço.")
            .MaximumLength(ContentMaxLength).WithMessage("O conteúdo é grande demais.");

        RuleFor(x => x.Audience)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o público-alvo.")
            .MaximumLength(AudienceMaxLength).WithMessage($"O público-alvo pode ter no máximo {AudienceMaxLength} caracteres.");

        RuleFor(x => x.WorkloadHours)
            .InclusiveBetween(1, WorkloadMaxHours).WithMessage($"A carga horária deve estar entre 1 e {WorkloadMaxHours} horas.");

        RuleFor(x => x.Format)
            .IsInEnum().WithMessage("Formato inválido. Use InPerson, Online ou InCompany.");

        RuleFor(x => x.CoverId)
            .NotEqual(Guid.Empty).When(x => x.CoverId is not null)
            .WithMessage("Capa inválida.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("A ordem não pode ser negativa.");
    }
}

/// <summary>Validates <see cref="CreateServiceRequest"/>.</summary>
public sealed class CreateServiceRequestValidator : ServiceFieldsValidator<CreateServiceRequest>;

/// <summary>Validates <see cref="UpdateServiceRequest"/>.</summary>
public sealed class UpdateServiceRequestValidator : ServiceFieldsValidator<UpdateServiceRequest>;
