using FluentValidation;

using Klf.Application.DTOs.Feedback;
using Klf.Application.Mappings;
using Klf.Application.Validators.Common;
using Klf.Domain.Entities;

namespace Klf.Application.Validators.Feedback;

internal static class FeedbackRules
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 1000;

    public static void Definition<T>(this IRuleBuilderInitial<T, FormDefinitionDto> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Informe as perguntas do formulário.")
            .Custom(CheckDefinition);

    private static void CheckDefinition<T>(FormDefinitionDto definition, ValidationContext<T> context)
    {
        foreach (var (field, message) in definition.ToDomain().FindProblems())
        {
            context.AddFailure($"Definition.{field}", message);
        }
    }
}

/// <summary>Rules for every request that carries <see cref="IFeedbackFormFields"/>, including the whole form structure.</summary>
/// <typeparam name="T">Request type.</typeparam>
public abstract class FeedbackFormFieldsValidator<T> : AbstractValidator<T>
    where T : IFeedbackFormFields
{
    /// <summary>Defines the shared rules.</summary>
    protected FeedbackFormFieldsValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o título do formulário.")
            .MaximumLength(FeedbackRules.TitleMaxLength).WithMessage($"O título pode ter no máximo {FeedbackRules.TitleMaxLength} caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(FeedbackRules.DescriptionMaxLength).WithMessage($"A descrição pode ter no máximo {FeedbackRules.DescriptionMaxLength} caracteres.");

        RuleFor(x => x.Definition).Definition();
    }
}

/// <summary>Validates <see cref="CreateFeedbackFormRequest"/>.</summary>
public sealed class CreateFeedbackFormRequestValidator : FeedbackFormFieldsValidator<CreateFeedbackFormRequest>;

/// <summary>Validates <see cref="UpdateFeedbackFormRequest"/>.</summary>
public sealed class UpdateFeedbackFormRequestValidator : FeedbackFormFieldsValidator<UpdateFeedbackFormRequest>;

/// <summary>Rules for every request that carries <see cref="IFeedbackSessionFields"/>.</summary>
/// <typeparam name="T">Request type.</typeparam>
public abstract class FeedbackSessionFieldsValidator<T> : AbstractValidator<T>
    where T : IFeedbackSessionFields
{
    /// <summary>Longest period a session may stay open.</summary>
    public static readonly TimeSpan MaxDuration = TimeSpan.FromDays(90);

    /// <summary>Largest response limit.</summary>
    public const int MaxResponsesLimit = 5000;

    /// <summary>Defines the shared rules.</summary>
    protected FeedbackSessionFieldsValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o nome da sessão.")
            .MaximumLength(FeedbackRules.TitleMaxLength).WithMessage($"O nome pode ter no máximo {FeedbackRules.TitleMaxLength} caracteres.");

        RuleFor(x => x.OpensAt)
            .NotEqual(default(DateTimeOffset)).WithMessage("Informe a abertura.");

        RuleFor(x => x.ClosesAt)
            .Cascade(CascadeMode.Stop)
            .NotEqual(default(DateTimeOffset)).WithMessage("Informe o encerramento.")
            .GreaterThan(x => x.OpensAt).WithMessage("O encerramento precisa ser depois da abertura.")
            .Must((request, closesAt) => closesAt - request.OpensAt <= MaxDuration)
            .WithMessage($"Uma sessão pode ficar aberta por no máximo {MaxDuration.Days} dias.");

        RuleFor(x => x.MaxResponses)
            .InclusiveBetween(1, MaxResponsesLimit).When(x => x.MaxResponses is not null)
            .WithMessage($"O limite de respostas deve estar entre 1 e {MaxResponsesLimit}.");

        RuleFor(x => x.ClientId)
            .NotEqual(Guid.Empty).When(x => x.ClientId is not null).WithMessage("Cliente inválido.");

        RuleFor(x => x.ServiceId)
            .NotEqual(Guid.Empty).When(x => x.ServiceId is not null).WithMessage("Serviço inválido.");
    }
}

/// <summary>Validates <see cref="CreateFeedbackSessionRequest"/>.</summary>
public sealed class CreateFeedbackSessionRequestValidator : FeedbackSessionFieldsValidator<CreateFeedbackSessionRequest>
{
    /// <summary>Defines the rules.</summary>
    public CreateFeedbackSessionRequestValidator()
    {
        RuleFor(x => x.FormId).NotEqual(Guid.Empty).WithMessage("Escolha o formulário.");
    }
}

/// <summary>Validates <see cref="UpdateFeedbackSessionRequest"/>.</summary>
public sealed class UpdateFeedbackSessionRequestValidator : FeedbackSessionFieldsValidator<UpdateFeedbackSessionRequest>;

/// <summary>Validates <see cref="ReplaceSessionFormRequest"/>.</summary>
public sealed class ReplaceSessionFormRequestValidator : AbstractValidator<ReplaceSessionFormRequest>
{
    /// <summary>Defines the rules.</summary>
    public ReplaceSessionFormRequestValidator()
    {
        RuleFor(x => x.FormTitle)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o título do formulário.")
            .MaximumLength(FeedbackRules.TitleMaxLength).WithMessage($"O título pode ter no máximo {FeedbackRules.TitleMaxLength} caracteres.");

        RuleFor(x => x.FormDescription)
            .MaximumLength(FeedbackRules.DescriptionMaxLength).WithMessage($"A descrição pode ter no máximo {FeedbackRules.DescriptionMaxLength} caracteres.");

        RuleFor(x => x.Definition).Definition();
    }
}

/// <summary>Validates <see cref="FeedbackSessionListRequest"/>.</summary>
public sealed class FeedbackSessionListRequestValidator : AbstractValidator<FeedbackSessionListRequest>
{
    /// <summary>Defines the rules.</summary>
    public FeedbackSessionListRequestValidator()
    {
        Include(new PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("A busca pode ter no máximo 100 caracteres.");
    }
}

/// <summary>Validates <see cref="FeedbackSummaryRequest"/>.</summary>
public sealed class FeedbackSummaryRequestValidator : AbstractValidator<FeedbackSummaryRequest>
{
    /// <summary>Defines the rules.</summary>
    public FeedbackSummaryRequestValidator()
    {
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From).When(x => x.From is not null && x.To is not null)
            .WithMessage("A data final precisa ser igual ou depois da inicial.");
    }
}

/// <summary>Validates the shape of <see cref="SubmitFeedbackRequest"/>. The answers are checked against the form by the domain.</summary>
public sealed class SubmitFeedbackRequestValidator : AbstractValidator<SubmitFeedbackRequest>
{
    /// <summary>Defines the rules.</summary>
    public SubmitFeedbackRequestValidator()
    {
        RuleFor(x => x.Answers)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Responda pelo menos uma pergunta.")
            .Must(answers => answers.Count <= FormDefinition.MaxQuestions).WithMessage("Respostas demais.");

        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Pergunta inválida.")
                .MaximumLength(40).WithMessage("Pergunta inválida.");

            answer.RuleFor(a => a.Text)
                .MaximumLength(FormDefinition.LongTextMaxLength)
                .WithMessage($"A resposta pode ter no máximo {FormDefinition.LongTextMaxLength} caracteres.");

            answer.RuleFor(a => a.Choices)
                .Must(choices => choices is null || choices.Count <= FormDefinition.MaxOptions).WithMessage("Opções demais.");
        });
    }
}
