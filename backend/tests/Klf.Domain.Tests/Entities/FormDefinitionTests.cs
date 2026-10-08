using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Tests.Entities;

public sealed class FormDefinitionTests
{
    [Fact]
    public void Sample_definition_has_no_problems()
    {
        Assert.Empty(FeedbackSamples.Definition().FindProblems());
    }

    [Fact]
    public void Problems_are_reported_when_form_is_empty_or_has_no_questions()
    {
        var empty = new FormDefinition([]);
        var noQuestions = new FormDefinition([new FormSection("s", "Seção", null, FeedbackTopic.Training, [])]);

        Assert.Contains(empty.FindProblems(), p => p.Message == "Adicione pelo menos uma seção.");
        Assert.Contains(noQuestions.FindProblems(), p => p.Message == "Adicione pelo menos uma pergunta.");
    }

    [Fact]
    public void Problems_are_reported_with_field_path_when_ids_repeat_or_are_invalid()
    {
        var definition = new FormDefinition(
        [
            new FormSection("s", "Seção", null, FeedbackTopic.Training,
            [
                FeedbackSamples.Question("q1", FeedbackQuestionType.ShortText),
                FeedbackSamples.Question("q1", FeedbackQuestionType.ShortText),
                FeedbackSamples.Question("com espaço", FeedbackQuestionType.ShortText),
            ]),
        ]);

        var problems = definition.FindProblems();

        Assert.Contains(problems, p => p.Field == "Sections[0].Questions[1].Id" && p.Message.Contains("repetido", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.Field == "Sections[0].Questions[2].Id");
    }

    [Fact]
    public void Problems_are_reported_when_choice_has_one_option_or_text_has_options()
    {
        var definition = new FormDefinition(
        [
            new FormSection("s", "Seção", null, FeedbackTopic.Training,
            [
                FeedbackSamples.Question("escolha", FeedbackQuestionType.SingleChoice) with { Options = [new("a", "A")] },
                FeedbackSamples.Question("texto", FeedbackQuestionType.ShortText) with { Options = [new("a", "A")] },
            ]),
        ]);

        var problems = definition.FindProblems();

        Assert.Contains(problems, p => p.Field == "Sections[0].Questions[0].Options");
        Assert.Contains(problems, p => p.Field == "Sections[0].Questions[1].Options");
    }

    [Theory]
    [InlineData(2, 5)]
    [InlineData(1, 11)]
    [InlineData(1, 1)]
    public void Problems_are_reported_when_scale_range_is_out_of_bounds(int min, int max)
    {
        var definition = new FormDefinition(
        [
            new FormSection("s", "Seção", null, FeedbackTopic.Training,
                [FeedbackSamples.Question("escala", FeedbackQuestionType.Scale) with { ScaleMin = min, ScaleMax = max }]),
        ]);

        Assert.Contains(definition.FindProblems(), p => p.Field == "Sections[0].Questions[0].ScaleMax");
    }

    [Fact]
    public void Problems_are_reported_when_condition_points_to_later_or_text_question_or_unknown_value()
    {
        var definition = new FormDefinition(
        [
            new FormSection("s", "Seção", null, FeedbackTopic.Training,
            [
                FeedbackSamples.Question("antes", FeedbackQuestionType.ShortText),
                FeedbackSamples.Question("a", FeedbackQuestionType.ShortText) with { ShowIf = new QuestionCondition("depois", ["1"]) },
                FeedbackSamples.Question("b", FeedbackQuestionType.ShortText) with { ShowIf = new QuestionCondition("antes", ["x"]) },
                FeedbackSamples.Question("nps", FeedbackQuestionType.Nps),
                FeedbackSamples.Question("c", FeedbackQuestionType.ShortText) with { ShowIf = new QuestionCondition("nps", ["11"]) },
                FeedbackSamples.Question("depois", FeedbackQuestionType.Nps),
            ]),
        ]);

        var problems = definition.FindProblems();

        Assert.Contains(problems, p => p.Field == "Sections[0].Questions[1].ShowIf.QuestionId" && p.Message.Contains("antes", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.Field == "Sections[0].Questions[2].ShowIf.QuestionId");
        Assert.Contains(problems, p => p.Field == "Sections[0].Questions[4].ShowIf.AnyOf");
    }

    [Fact]
    public void Ensure_valid_throws_with_definition_prefix_on_each_field()
    {
        var error = Assert.Throws<ValidationException>(() => new FormDefinition([]).EnsureValid());

        Assert.Contains("Definition.Sections", error.Errors.Keys);
    }

    [Fact]
    public void Normalize_keeps_valid_answers_trimmed_and_drops_empty_ones()
    {
        var answers = FeedbackSamples.Definition().NormalizeAnswers(
        [
            new FeedbackAnswer("bom", "  Equipe unida  ", null, null),
            new FeedbackAnswer("melhorar", "   ", null, null),
            new FeedbackAnswer("nota", null, 4, null),
            new FeedbackAnswer("nps", null, 10, null),
            new FeedbackAnswer("temas", null, null, ["a", "c"]),
        ]);

        Assert.Equal(["bom", "nota", "nps", "temas"], answers.Select(a => a.QuestionId));
        Assert.Equal("Equipe unida", answers[0].Text);
    }

    [Fact]
    public void Normalize_drops_answer_of_question_hidden_by_condition()
    {
        var answers = FeedbackSamples.Definition().NormalizeAnswers(
            [.. FeedbackSamples.ValidAnswers(nps: 10), new FeedbackAnswer("porque", "não deveria ficar", null, null)]);

        Assert.DoesNotContain(answers, a => a.QuestionId == "porque");
    }

    [Fact]
    public void Normalize_keeps_answer_of_question_shown_by_condition()
    {
        var answers = FeedbackSamples.Definition().NormalizeAnswers(
            [.. FeedbackSamples.ValidAnswers(nps: 3), new FeedbackAnswer("porque", "Faltou prática", null, null)]);

        Assert.Contains(answers, a => a.QuestionId == "porque" && a.Text == "Faltou prática");
    }

    [Fact]
    public void Normalize_reports_every_problem_by_question_key()
    {
        var error = Assert.Throws<ValidationException>(() => FeedbackSamples.Definition().NormalizeAnswers(
        [
            new FeedbackAnswer("nota", null, 9, null),
            new FeedbackAnswer("nps", "dez", null, null),
            new FeedbackAnswer("formato", null, null, ["presencial", "online"]),
            new FeedbackAnswer("temas", null, null, ["z"]),
            new FeedbackAnswer("inventada", "x", null, null),
        ]));

        Assert.Equal(["Responda esta pergunta."], error.Errors["Answers.bom"]);
        Assert.Equal(["Escolha um valor de 1 a 5."], error.Errors["Answers.nota"]);
        Assert.Equal(["Resposta inválida para esta pergunta."], error.Errors["Answers.nps"]);
        Assert.Equal(["Escolha apenas uma opção."], error.Errors["Answers.formato"]);
        Assert.Equal(["Opção inválida."], error.Errors["Answers.temas"]);
        Assert.Equal(["Pergunta desconhecida."], error.Errors["Answers.inventada"]);
    }

    [Fact]
    public void Normalize_rejects_repeated_answers_and_text_over_limit()
    {
        var error = Assert.Throws<ValidationException>(() => FeedbackSamples.Definition().NormalizeAnswers(
        [
            new FeedbackAnswer("bom", new string('a', FormDefinition.LongTextMaxLength + 1), null, null),
            new FeedbackAnswer("nota", null, 3, null),
            new FeedbackAnswer("nota", null, 4, null),
            new FeedbackAnswer("nps", null, 8, null),
        ]));

        Assert.Contains("Answers.bom", error.Errors.Keys);
        Assert.Contains("Answers.nota", error.Errors.Keys);
    }

    [Fact]
    public void Normalize_requires_at_least_one_answer_when_every_question_is_optional()
    {
        var definition = new FormDefinition([new FormSection("s", "S", null, FeedbackTopic.Training, [FeedbackSamples.Question("q", FeedbackQuestionType.ShortText)])]);

        var error = Assert.Throws<ValidationException>(() => definition.NormalizeAnswers([]));

        Assert.Contains("Answers", error.Errors.Keys);
    }
}
