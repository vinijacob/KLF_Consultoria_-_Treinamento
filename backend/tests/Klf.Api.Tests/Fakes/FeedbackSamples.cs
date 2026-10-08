using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Api.Tests.Fakes;

public static class FeedbackSamples
{
    public static FormDefinition Definition() => new(
    [
        new FormSection("empresa", "Sobre a sua empresa", null, FeedbackTopic.Company,
        [
            Question("bom", FeedbackQuestionType.LongText, required: true),
            Question("melhorar", FeedbackQuestionType.LongText),
        ]),
        new FormSection("klf", "Sobre o treinamento", null, FeedbackTopic.Training,
        [
            Question("nota", FeedbackQuestionType.Scale, required: true) with { ScaleMin = 1, ScaleMax = 5 },
            Question("nps", FeedbackQuestionType.Nps, required: true),
            Question("porque", FeedbackQuestionType.LongText) with { ShowIf = new QuestionCondition("nps", ["0", "1", "2", "3", "4", "5", "6"]) },
            Question("formato", FeedbackQuestionType.SingleChoice) with { Options = [new("presencial", "Presencial"), new("online", "Online")] },
            Question("temas", FeedbackQuestionType.MultipleChoice) with { Options = [new("a", "Vendas"), new("b", "Atendimento"), new("c", "Liderança")] },
        ]),
    ]);

    public static IReadOnlyList<FeedbackAnswer> ValidAnswers(int nps = 9, string bom = "Equipe unida") =>
    [
        new("bom", bom, null, null),
        new("nota", null, 5, null),
        new("nps", null, nps, null),
    ];

    public static FormQuestion Question(string id, FeedbackQuestionType type, bool required = false) =>
        new(id, type, $"Pergunta {id}", null, required, [], null, null, null, null, null);
}
