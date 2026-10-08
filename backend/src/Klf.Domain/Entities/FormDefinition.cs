using System.Globalization;
using System.Text.RegularExpressions;

using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Entities;

/// <summary>
/// The questions of a feedback form, organized in sections. Stored as JSON: a template in <see cref="FeedbackForm"/>
/// and a frozen copy in each <see cref="FeedbackSession"/>, so editing a template never changes answered sessions.
/// </summary>
/// <param name="Sections">Sections in display order.</param>
public sealed partial record FormDefinition(IReadOnlyList<FormSection> Sections)
{
    /// <summary>Largest number of sections.</summary>
    public const int MaxSections = 20;

    /// <summary>Largest number of questions in the whole form.</summary>
    public const int MaxQuestions = 100;

    /// <summary>Largest number of options of a choice question.</summary>
    public const int MaxOptions = 30;

    /// <summary>Largest length of a short text answer.</summary>
    public const int ShortTextMaxLength = 300;

    /// <summary>Largest length of a long text answer.</summary>
    public const int LongTextMaxLength = 3000;

    /// <summary>Every question, in display order.</summary>
    public IEnumerable<FormQuestion> AllQuestions() => Sections.SelectMany(section => section.Questions);

    /// <summary>
    /// Lists every structural problem (empty form, duplicated ids, a choice question without options, a condition that points
    /// to a later question...), as (field path, message in Portuguese). An empty list means the definition is valid.
    /// </summary>
    public IReadOnlyList<(string Field, string Message)> FindProblems()
    {
        var problems = new List<(string, string)>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var earlier = new Dictionary<string, FormQuestion>(StringComparer.Ordinal);

        if (Sections is null || Sections.Count == 0)
        {
            return [("Sections", "Adicione pelo menos uma seção.")];
        }

        if (Sections.Count > MaxSections)
        {
            problems.Add(("Sections", $"O formulário pode ter no máximo {MaxSections} seções."));
        }

        var questionCount = Sections.Sum(section => section.Questions?.Count ?? 0);

        if (questionCount == 0)
        {
            problems.Add(("Sections", "Adicione pelo menos uma pergunta."));
        }
        else if (questionCount > MaxQuestions)
        {
            problems.Add(("Sections", $"O formulário pode ter no máximo {MaxQuestions} perguntas."));
        }

        for (var s = 0; s < Sections.Count; s++)
        {
            var section = Sections[s];
            var path = $"Sections[{s}]";

            CheckId(section.Id, $"{path}.Id", seenIds, problems);
            CheckText(section.Title, $"{path}.Title", "o título da seção", 200, required: true, problems);
            CheckText(section.Description, $"{path}.Description", "a descrição da seção", 1000, required: false, problems);

            if (!Enum.IsDefined(section.Topic))
            {
                problems.Add(($"{path}.Topic", "Assunto inválido. Use Company ou Training."));
            }

            for (var q = 0; q < (section.Questions?.Count ?? 0); q++)
            {
                var question = section.Questions![q];
                CheckQuestion(question, $"{path}.Questions[{q}]", seenIds, earlier, problems);
                earlier.TryAdd(question.Id ?? string.Empty, question);
            }
        }

        return problems;
    }

    /// <summary>Throws when <see cref="FindProblems"/> finds any problem.</summary>
    /// <exception cref="ValidationException">The definition is invalid; errors are grouped by field path.</exception>
    public void EnsureValid()
    {
        var problems = FindProblems();

        if (problems.Count > 0)
        {
            throw new ValidationException(problems
                .GroupBy(problem => $"Definition.{problem.Field}")
                .ToDictionary(group => group.Key, group => group.Select(problem => problem.Message).ToArray()));
        }
    }

    /// <summary>
    /// Checks the answers of a respondent against the form and returns them cleaned: answers to questions hidden by a condition
    /// are dropped, text is trimmed and empty answers are removed.
    /// </summary>
    /// <param name="answers">Answers as sent by the respondent.</param>
    /// <exception cref="ValidationException">
    /// Unknown or repeated question, a required visible question without answer, or an answer of the wrong kind or out of range.
    /// Errors use the key <c>Answers.{questionId}</c>.
    /// </exception>
    public IReadOnlyList<FeedbackAnswer> NormalizeAnswers(IReadOnlyList<FeedbackAnswer> answers)
    {
        var errors = new Dictionary<string, string[]>();
        var known = AllQuestions().Select(question => question.Id).ToHashSet(StringComparer.Ordinal);
        var byQuestion = new Dictionary<string, FeedbackAnswer>(StringComparer.Ordinal);

        foreach (var answer in answers)
        {
            if (!known.Contains(answer.QuestionId))
            {
                errors[$"Answers.{answer.QuestionId}"] = ["Pergunta desconhecida."];
            }
            else if (!byQuestion.TryAdd(answer.QuestionId, answer))
            {
                errors[$"Answers.{answer.QuestionId}"] = ["A pergunta foi respondida mais de uma vez."];
            }
        }

        var kept = new Dictionary<string, FeedbackAnswer>(StringComparer.Ordinal);

        foreach (var question in AllQuestions())
        {
            if (question.ShowIf is { } condition && !IsMet(condition, kept))
            {
                continue;
            }

            byQuestion.TryGetValue(question.Id, out var answer);
            var cleaned = answer is null ? null : Clean(answer);

            if (cleaned is null)
            {
                if (question.Required)
                {
                    errors[$"Answers.{question.Id}"] = ["Responda esta pergunta."];
                }

                continue;
            }

            var error = CheckAnswer(question, cleaned);

            if (error is null)
            {
                kept[question.Id] = cleaned;
            }
            else
            {
                errors[$"Answers.{question.Id}"] = [error];
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        if (kept.Count == 0)
        {
            throw new ValidationException("Answers", "Responda pelo menos uma pergunta.");
        }

        return [.. kept.Values];
    }

    [GeneratedRegex("^[a-zA-Z0-9_-]{1,40}$")]
    private static partial Regex IdPattern();

    private static bool IsMet(QuestionCondition condition, Dictionary<string, FeedbackAnswer> answered)
    {
        if (!answered.TryGetValue(condition.QuestionId, out var answer))
        {
            return false;
        }

        if (answer.Number is { } number)
        {
            return condition.AnyOf.Contains(number.ToString(CultureInfo.InvariantCulture));
        }

        return answer.Choices?.Any(choice => condition.AnyOf.Contains(choice)) == true;
    }

    private static FeedbackAnswer? Clean(FeedbackAnswer answer)
    {
        var text = string.IsNullOrWhiteSpace(answer.Text) ? null : answer.Text.Trim();
        var choices = answer.Choices is { Count: > 0 } ? answer.Choices : null;

        return text is null && answer.Number is null && choices is null
            ? null
            : answer with { Text = text, Choices = choices };
    }

    private static string? CheckAnswer(FormQuestion question, FeedbackAnswer answer)
    {
        const string wrongKind = "Resposta inválida para esta pergunta.";

        switch (question.Type)
        {
            case FeedbackQuestionType.ShortText or FeedbackQuestionType.LongText:
                var max = question.Type == FeedbackQuestionType.ShortText ? ShortTextMaxLength : LongTextMaxLength;
                return answer.Text is null || answer.Number is not null || answer.Choices is not null
                    ? wrongKind
                    : answer.Text.Length > max ? $"A resposta pode ter no máximo {max} caracteres." : null;

            case FeedbackQuestionType.Scale or FeedbackQuestionType.Nps:
                var (min, top) = question.GetNumericRange()!.Value;
                return answer.Number is not { } value || answer.Text is not null || answer.Choices is not null
                    ? wrongKind
                    : value < min || value > top ? $"Escolha um valor de {min} a {top}." : null;

            case FeedbackQuestionType.SingleChoice or FeedbackQuestionType.MultipleChoice:
                if (answer.Choices is null || answer.Text is not null || answer.Number is not null)
                {
                    return wrongKind;
                }

                var valid = question.Options.Select(option => option.Id).ToHashSet(StringComparer.Ordinal);

                if (answer.Choices.Any(choice => !valid.Contains(choice)) || answer.Choices.Distinct().Count() != answer.Choices.Count)
                {
                    return "Opção inválida.";
                }

                return question.Type == FeedbackQuestionType.SingleChoice && answer.Choices.Count != 1 ? "Escolha apenas uma opção." : null;

            default:
                return wrongKind;
        }
    }

    private static void CheckId(string? id, string field, HashSet<string> seen, List<(string, string)> problems)
    {
        if (string.IsNullOrEmpty(id) || !IdPattern().IsMatch(id))
        {
            problems.Add((field, "Identificador inválido: use de 1 a 40 letras, números, '-' ou '_'."));
        }
        else if (!seen.Add(id))
        {
            problems.Add((field, $"O identificador '{id}' está repetido."));
        }
    }

    private static void CheckText(string? text, string field, string name, int max, bool required, List<(string, string)> problems)
    {
        if (required && string.IsNullOrWhiteSpace(text))
        {
            problems.Add((field, $"Informe {name}."));
        }
        else if (text?.Length > max)
        {
            problems.Add((field, $"{char.ToUpperInvariant(name[0])}{name[1..]} pode ter no máximo {max} caracteres."));
        }
    }

    private static void CheckQuestion(
        FormQuestion question,
        string path,
        HashSet<string> seenIds,
        Dictionary<string, FormQuestion> earlier,
        List<(string, string)> problems)
    {
        CheckId(question.Id, $"{path}.Id", seenIds, problems);
        CheckText(question.Text, $"{path}.Text", "o texto da pergunta", 500, required: true, problems);
        CheckText(question.HelpText, $"{path}.HelpText", "o texto de ajuda", 500, required: false, problems);
        CheckText(question.MinLabel, $"{path}.MinLabel", "o rótulo do menor valor", 60, required: false, problems);
        CheckText(question.MaxLabel, $"{path}.MaxLabel", "o rótulo do maior valor", 60, required: false, problems);

        if (!Enum.IsDefined(question.Type))
        {
            problems.Add(($"{path}.Type", "Tipo de pergunta inválido."));
            return;
        }

        var options = question.Options ?? [];
        var isChoice = question.Type is FeedbackQuestionType.SingleChoice or FeedbackQuestionType.MultipleChoice;

        if (isChoice)
        {
            if (options.Count is < 2 or > MaxOptions)
            {
                problems.Add(($"{path}.Options", $"Perguntas de escolha precisam de 2 a {MaxOptions} opções."));
            }

            var optionIds = new HashSet<string>(StringComparer.Ordinal);

            for (var o = 0; o < options.Count; o++)
            {
                CheckId(options[o].Id, $"{path}.Options[{o}].Id", optionIds, problems);
                CheckText(options[o].Label, $"{path}.Options[{o}].Label", "o texto da opção", 200, required: true, problems);
            }
        }
        else if (options.Count > 0)
        {
            problems.Add(($"{path}.Options", "Só perguntas de escolha têm opções."));
        }

        if (question.Type == FeedbackQuestionType.Scale)
        {
            var min = question.ScaleMin ?? 1;
            var max = question.ScaleMax ?? 5;

            if (min is < 0 or > 1 || max is < 2 or > 10)
            {
                problems.Add(($"{path}.ScaleMax", "A escala começa em 0 ou 1 e termina entre 2 e 10."));
            }
        }
        else if (question.ScaleMin is not null || question.ScaleMax is not null)
        {
            problems.Add(($"{path}.ScaleMin", "Só perguntas de escala têm início e fim configuráveis."));
        }

        if (question.ShowIf is { } condition)
        {
            CheckCondition(condition, $"{path}.ShowIf", earlier, problems);
        }
    }

    private static void CheckCondition(
        QuestionCondition condition,
        string path,
        Dictionary<string, FormQuestion> earlier,
        List<(string, string)> problems)
    {
        if (condition.QuestionId is null || !earlier.TryGetValue(condition.QuestionId, out var target))
        {
            problems.Add(($"{path}.QuestionId", "A condição precisa apontar para uma pergunta que vem antes desta."));
            return;
        }

        if (target.Type is FeedbackQuestionType.ShortText or FeedbackQuestionType.LongText)
        {
            problems.Add(($"{path}.QuestionId", "Condições só podem depender de perguntas de escala, NPS ou escolha."));
            return;
        }

        if (condition.AnyOf is null || condition.AnyOf.Count == 0)
        {
            problems.Add(($"{path}.AnyOf", "Informe pelo menos um valor que mostra a pergunta."));
            return;
        }

        var allowed = target.GetNumericRange() is { } range
            ? Enumerable.Range(range.Min, range.Max - range.Min + 1).Select(value => value.ToString(CultureInfo.InvariantCulture)).ToHashSet()
            : (target.Options ?? []).Select(option => option.Id).ToHashSet(StringComparer.Ordinal);

        if (condition.AnyOf.Any(value => !allowed.Contains(value)))
        {
            problems.Add(($"{path}.AnyOf", "A condição usa um valor que a pergunta de origem não tem."));
        }
    }
}

/// <summary>A section of a feedback form.</summary>
/// <param name="Id">Identifier chosen by the form builder (letters, numbers, '-' or '_').</param>
/// <param name="Title">Section title.</param>
/// <param name="Description">Text shown under the title; optional.</param>
/// <param name="Topic">Whether the section is about the trainee's company or about KLF and the training.</param>
/// <param name="Questions">Questions in display order.</param>
public sealed record FormSection(string Id, string Title, string? Description, FeedbackTopic Topic, IReadOnlyList<FormQuestion> Questions);

/// <summary>A question of a feedback form.</summary>
/// <param name="Id">Identifier chosen by the form builder, unique in the form; answers and conditions refer to it.</param>
/// <param name="Type">Kind of question.</param>
/// <param name="Text">The question.</param>
/// <param name="HelpText">Extra explanation; optional.</param>
/// <param name="Required">Whether it must be answered when visible.</param>
/// <param name="Options">Options of a choice question; empty for other kinds.</param>
/// <param name="ScaleMin">First value of a scale question (0 or 1; default 1).</param>
/// <param name="ScaleMax">Last value of a scale question (2 to 10; default 5).</param>
/// <param name="MinLabel">Label of the lowest value (e.g. "Ruim"); optional.</param>
/// <param name="MaxLabel">Label of the highest value (e.g. "Excelente"); optional.</param>
/// <param name="ShowIf">Shows the question only when an earlier question got one of the given answers; optional.</param>
public sealed record FormQuestion(
    string Id,
    FeedbackQuestionType Type,
    string Text,
    string? HelpText,
    bool Required,
    IReadOnlyList<FormOption> Options,
    int? ScaleMin,
    int? ScaleMax,
    string? MinLabel,
    string? MaxLabel,
    QuestionCondition? ShowIf)
{
    /// <summary>Accepted range for scale and NPS questions; <see langword="null"/> for the other kinds.</summary>
    public (int Min, int Max)? GetNumericRange() => Type switch
    {
        FeedbackQuestionType.Nps => (0, 10),
        FeedbackQuestionType.Scale => (ScaleMin ?? 1, ScaleMax ?? 5),
        _ => null,
    };
}

/// <summary>An option of a choice question.</summary>
/// <param name="Id">Identifier chosen by the form builder, unique in the question.</param>
/// <param name="Label">Text shown to the respondent.</param>
public sealed record FormOption(string Id, string Label);

/// <summary>Shows a question only when an earlier scale, NPS or choice question got one of the given answers.</summary>
/// <param name="QuestionId">The earlier question.</param>
/// <param name="AnyOf">Values that show the question: option ids, or numbers as text (e.g. "0" to "6" for NPS detractors).</param>
public sealed record QuestionCondition(string QuestionId, IReadOnlyList<string> AnyOf);

/// <summary>An answer to one question. Exactly one of the value fields is filled, according to the question type.</summary>
/// <param name="QuestionId">The question.</param>
/// <param name="Text">Text answer (short and long text questions).</param>
/// <param name="Number">Numeric answer (scale and NPS questions).</param>
/// <param name="Choices">Chosen option ids (choice questions).</param>
public sealed record FeedbackAnswer(string QuestionId, string? Text, int? Number, IReadOnlyList<string>? Choices);
