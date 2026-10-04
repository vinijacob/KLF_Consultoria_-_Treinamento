using FluentValidation;

using Klf.Application.DTOs.Albums;

namespace Klf.Application.Validators.Albums;

/// <summary>Rules for every request that carries <see cref="IAlbumFields"/>.</summary>
/// <typeparam name="T">Request type.</typeparam>
public abstract class AlbumFieldsValidator<T> : AbstractValidator<T>
    where T : IAlbumFields
{
    /// <summary>Largest title length, matching the database column.</summary>
    public const int TitleMaxLength = 200;

    /// <summary>Largest slug length, matching the database column.</summary>
    public const int SlugMaxLength = 200;

    /// <summary>Largest description length, matching the database column.</summary>
    public const int DescriptionMaxLength = 500;

    /// <summary>Defines the shared rules.</summary>
    protected AlbumFieldsValidator()
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

        RuleFor(x => x.Description)
            .MaximumLength(DescriptionMaxLength).WithMessage($"A descrição pode ter no máximo {DescriptionMaxLength} caracteres.");

        RuleFor(x => x.CoverId)
            .NotEqual(Guid.Empty).When(x => x.CoverId is not null)
            .WithMessage("Capa inválida.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("A ordem não pode ser negativa.");
    }
}

/// <summary>Validates <see cref="CreateAlbumRequest"/>.</summary>
public sealed class CreateAlbumRequestValidator : AlbumFieldsValidator<CreateAlbumRequest>;

/// <summary>Validates <see cref="UpdateAlbumRequest"/>.</summary>
public sealed class UpdateAlbumRequestValidator : AlbumFieldsValidator<UpdateAlbumRequest>;

/// <summary>Validates <see cref="SetAlbumItemsRequest"/>.</summary>
public sealed class SetAlbumItemsRequestValidator : AbstractValidator<SetAlbumItemsRequest>
{
    /// <summary>Largest number of images in one album.</summary>
    public const int MaxItems = 200;

    /// <summary>Largest caption length, matching the database column.</summary>
    public const int CaptionMaxLength = 300;

    /// <summary>Defines the rules.</summary>
    public SetAlbumItemsRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Informe a lista de imagens.")
            .Must(items => items is null || items.Count <= MaxItems).WithMessage($"Um álbum pode ter no máximo {MaxItems} imagens.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MediaAssetId).NotEqual(Guid.Empty).WithMessage("Imagem inválida.");
            item.RuleFor(i => i.Caption).MaximumLength(CaptionMaxLength).WithMessage($"A legenda pode ter no máximo {CaptionMaxLength} caracteres.");
        });
    }
}
