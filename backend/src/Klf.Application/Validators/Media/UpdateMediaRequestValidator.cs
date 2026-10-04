using FluentValidation;

using Klf.Application.DTOs.Media;

namespace Klf.Application.Validators.Media;

/// <summary>Validates <see cref="UpdateMediaRequest"/>.</summary>
public sealed class UpdateMediaRequestValidator : AbstractValidator<UpdateMediaRequest>
{
    /// <summary>Largest text alternative length, matching the database column.</summary>
    public const int AltTextMaxLength = 200;

    /// <summary>Defines the rules.</summary>
    public UpdateMediaRequestValidator()
    {
        RuleFor(x => x.AltText)
            .MaximumLength(AltTextMaxLength).WithMessage($"O texto alternativo pode ter no máximo {AltTextMaxLength} caracteres.");
    }
}
