using System.Text.Json;

using FluentValidation;

using Klf.Application.DTOs.Posts;
using Klf.Domain.Enums;

namespace Klf.Application.Validators.Posts;

/// <summary>Rules for every request that carries <see cref="IPostFields"/>.</summary>
/// <typeparam name="T">Request type.</typeparam>
public abstract class PostFieldsValidator<T> : AbstractValidator<T>
    where T : IPostFields
{
    /// <summary>Largest title length, matching the database column.</summary>
    public const int TitleMaxLength = 200;

    /// <summary>Largest slug length, matching the database column.</summary>
    public const int SlugMaxLength = 200;

    /// <summary>Largest summary length, matching the database column.</summary>
    public const int SummaryMaxLength = 500;

    /// <summary>Largest SEO title length, matching the database column.</summary>
    public const int SeoTitleMaxLength = 60;

    /// <summary>Largest SEO description length, matching the database column.</summary>
    public const int SeoDescriptionMaxLength = 160;

    /// <summary>Largest body size (JSON or HTML), so a single post cannot grow without limit.</summary>
    public const int ContentMaxLength = 200_000;

    /// <summary>Defines the shared rules.</summary>
    protected PostFieldsValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de post inválido. Use Project, Article ou News.");

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

        RuleFor(x => x.ContentJson)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o conteúdo.")
            .MaximumLength(ContentMaxLength).WithMessage("O conteúdo é grande demais.")
            .Must(BeValidJson).WithMessage("O conteúdo do editor não é um JSON válido.");

        RuleFor(x => x.ContentHtml)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o conteúdo em HTML.")
            .MaximumLength(ContentMaxLength).WithMessage("O conteúdo é grande demais.");

        RuleFor(x => x.CoverId)
            .NotEqual(Guid.Empty).When(x => x.CoverId is not null)
            .WithMessage("Capa inválida.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido. Use Draft, Scheduled ou Published.");

        RuleFor(x => x.ScheduledFor)
            .NotNull().When(x => x.Status == PostStatus.Scheduled)
            .WithMessage("Informe a data do agendamento.");

        RuleFor(x => x.SeoTitle)
            .MaximumLength(SeoTitleMaxLength).WithMessage($"O título SEO pode ter no máximo {SeoTitleMaxLength} caracteres.");

        RuleFor(x => x.SeoDescription)
            .MaximumLength(SeoDescriptionMaxLength).WithMessage($"A descrição SEO pode ter no máximo {SeoDescriptionMaxLength} caracteres.");
    }

    private static bool BeValidJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

/// <summary>Validates <see cref="CreatePostRequest"/>.</summary>
public sealed class CreatePostRequestValidator : PostFieldsValidator<CreatePostRequest>;

/// <summary>Validates <see cref="UpdatePostRequest"/>.</summary>
public sealed class UpdatePostRequestValidator : PostFieldsValidator<UpdatePostRequest>;
