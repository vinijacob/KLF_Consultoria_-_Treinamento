using FluentValidation;

using Klf.Application.DTOs.Posts;
using Klf.Application.Services.Posts;

namespace Klf.Application.Validators.Posts;

/// <summary>Rules shared by the requests that name a category or tag.</summary>
/// <typeparam name="T">Request type.</typeparam>
public abstract class TaxonomyNameValidator<T> : AbstractValidator<T>
    where T : ITaxonomyNameFields
{
    /// <summary>Defines the rules.</summary>
    /// <param name="maxLength">Largest name.</param>
    protected TaxonomyNameValidator(int maxLength)
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o nome.")
            .Must(name => TaxonomyText.CleanName(name).Length <= maxLength).WithMessage($"O nome pode ter no máximo {maxLength} caracteres.")
            .Must(name => TaxonomyText.ToSlug(name).Length > 0).WithMessage("O nome precisa ter pelo menos uma letra ou um número.");
    }
}

/// <summary>Validates <see cref="CreateCategoryRequest"/>.</summary>
public sealed class CreateCategoryRequestValidator() : TaxonomyNameValidator<CreateCategoryRequest>(TaxonomyText.CategoryNameMaxLength);

/// <summary>Validates <see cref="UpdateCategoryRequest"/>.</summary>
public sealed class UpdateCategoryRequestValidator() : TaxonomyNameValidator<UpdateCategoryRequest>(TaxonomyText.CategoryNameMaxLength);

/// <summary>Validates <see cref="UpdateTagRequest"/>.</summary>
public sealed class UpdateTagRequestValidator() : TaxonomyNameValidator<UpdateTagRequest>(TaxonomyText.TagNameMaxLength);
