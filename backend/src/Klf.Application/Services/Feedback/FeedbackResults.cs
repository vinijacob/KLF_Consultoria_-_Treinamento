using System.Security.Cryptography;

using Klf.Application.DTOs.Feedback;
using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Application.Services.Feedback;

/// <summary>Turns anonymous responses into aggregated results, hiding anything based on too few people.</summary>
internal static class FeedbackResults
{
    /// <summary>Responses needed before a session, or a single question, shows anything beyond its count.</summary>
    public const int MinimumResponses = 3;

    public static FeedbackSessionResultsResponse ForSession(
        FeedbackSession session,
        FeedbackSessionStatus status,
        IReadOnlyList<FeedbackResponse> responses)
    {
        if (responses.Count < MinimumResponses)
        {
            return new FeedbackSessionResultsResponse(session.Id, session.Title, status, responses.Count, MinimumResponses, false, null, []);
        }

        var answers = responses
            .SelectMany(response => response.Answers)
            .GroupBy(answer => answer.QuestionId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var sections = session.Definition.Sections
            .Select(section => new SectionResult(
                section.Id,
                section.Title,
                section.Topic,
                [.. section.Questions.Select(question => ForQuestion(question, answers.GetValueOrDefault(question.Id) ?? []))]))
            .ToList();

        return new FeedbackSessionResultsResponse(
            session.Id,
            session.Title,
            status,
            responses.Count,
            MinimumResponses,
            true,
            PrimaryNps(session, responses),
            sections);
    }

    /// <summary>NPS of the main NPS question of a session, or <see langword="null"/> if it has none or too few answers.</summary>
    public static NpsResult? PrimaryNps(FeedbackSession session, IEnumerable<FeedbackResponse> responses)
    {
        var values = PrimaryNpsValues(session, responses);

        return values.Count >= MinimumResponses ? Nps(values) : null;
    }

    /// <summary>Answers to the main NPS question: the first NPS question about the training, or else the first NPS question.</summary>
    public static IReadOnlyList<int> PrimaryNpsValues(FeedbackSession session, IEnumerable<FeedbackResponse> responses)
    {
        var question = session.Definition.Sections
            .Where(section => section.Topic == FeedbackTopic.Training)
            .SelectMany(section => section.Questions)
            .FirstOrDefault(q => q.Type == FeedbackQuestionType.Nps)
            ?? session.Definition.AllQuestions().FirstOrDefault(q => q.Type == FeedbackQuestionType.Nps);

        if (question is null)
        {
            return [];
        }

        return [.. responses
            .SelectMany(response => response.Answers)
            .Where(answer => answer.QuestionId == question.Id && answer.Number is not null)
            .Select(answer => answer.Number!.Value)];
    }

    public static NpsResult Nps(IReadOnlyCollection<int> values)
    {
        var promoters = values.Count(value => value >= 9);
        var detractors = values.Count(value => value <= 6);
        var passives = values.Count - promoters - detractors;
        var score = (int)Math.Round((promoters - detractors) * 100.0 / values.Count, MidpointRounding.AwayFromZero);

        return new NpsResult(score, promoters, passives, detractors, values.Count);
    }

    private static QuestionResult ForQuestion(FormQuestion question, List<FeedbackAnswer> answers)
    {
        if (answers.Count < MinimumResponses)
        {
            return new QuestionResult(question.Id, question.Text, question.Type, answers.Count, true, null, null, null, null, null);
        }

        switch (question.Type)
        {
            case FeedbackQuestionType.Scale or FeedbackQuestionType.Nps:
                var numbers = answers.Where(a => a.Number is not null).Select(a => a.Number!.Value).ToList();
                var (min, max) = question.GetNumericRange()!.Value;
                var distribution = Enumerable.Range(min, max - min + 1)
                    .Select(value => new ValueCount(value, numbers.Count(n => n == value)))
                    .ToList();

                return new QuestionResult(
                    question.Id,
                    question.Text,
                    question.Type,
                    answers.Count,
                    false,
                    Math.Round(numbers.Average(), 2),
                    distribution,
                    question.Type == FeedbackQuestionType.Nps ? Nps(numbers) : null,
                    null,
                    null);

            case FeedbackQuestionType.SingleChoice or FeedbackQuestionType.MultipleChoice:
                var chosen = answers.SelectMany(a => a.Choices ?? []).ToList();
                var options = question.Options
                    .Select(option => new OptionCount(option.Id, option.Label, chosen.Count(c => c == option.Id)))
                    .ToList();

                return new QuestionResult(question.Id, question.Text, question.Type, answers.Count, false, null, null, null, options, null);

            default:
                var texts = answers.Where(a => a.Text is not null).Select(a => a.Text!).ToArray();
                RandomNumberGenerator.Shuffle(texts.AsSpan());

                return new QuestionResult(question.Id, question.Text, question.Type, answers.Count, false, null, null, null, null, texts);
        }
    }
}
