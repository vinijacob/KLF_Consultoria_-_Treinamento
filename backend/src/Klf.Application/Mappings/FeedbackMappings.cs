using Klf.Application.DTOs.Feedback;
using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Application.Mappings;

internal static class FeedbackMappings
{
    public static FormDefinition ToDomain(this FormDefinitionDto dto) => new(
        [.. (dto.Sections ?? []).Select(section => new FormSection(
            section.Id,
            section.Title?.Trim()!,
            NullIfBlank(section.Description),
            section.Topic,
            [.. (section.Questions ?? []).Select(ToDomain)]))]);

    public static FormDefinitionDto ToDto(this FormDefinition definition) => new(
        [.. definition.Sections.Select(section => new FormSectionDto(
            section.Id,
            section.Title,
            section.Description,
            section.Topic,
            [.. section.Questions.Select(ToDto)]))]);

    public static FeedbackFormResponse ToResponse(this FeedbackForm form) =>
        new(form.Id, form.Title, form.Description, form.Definition.ToDto());

    public static FeedbackFormListItemResponse ToListItem(this FeedbackForm form) => new(
        form.Id,
        form.Title,
        form.Description,
        form.Definition.Sections.Count,
        form.Definition.AllQuestions().Count());

    public static FeedbackSessionResponse ToResponse(this FeedbackSession session, FeedbackSessionStatus status, int responseCount, Uri publicUrl) => new(
        session.Id,
        session.Title,
        session.PublicCode,
        publicUrl.AbsoluteUri,
        status,
        responseCount,
        session.FormId,
        session.FormTitle,
        session.FormDescription,
        session.Definition.ToDto(),
        Utc(session.OpensAt),
        Utc(session.ClosesAt),
        session.MaxResponses,
        session.ClosedAt is { } closedAt ? Utc(closedAt) : null,
        session.ClientId,
        session.ServiceId,
        session.OwnerId);

    public static FeedbackSessionListItemResponse ToListItem(this FeedbackSession session, FeedbackSessionStatus status, int responseCount) => new(
        session.Id,
        session.Title,
        session.PublicCode,
        status,
        responseCount,
        Utc(session.OpensAt),
        Utc(session.ClosesAt),
        session.MaxResponses,
        session.ClientId,
        session.ServiceId);

    public static FeedbackAnswer ToDomain(this FeedbackAnswerDto dto) =>
        new(dto.QuestionId, dto.Text, dto.Number, dto.Choices);

    public static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static FormQuestion ToDomain(FormQuestionDto question) => new(
        question.Id,
        question.Type,
        question.Text?.Trim()!,
        NullIfBlank(question.HelpText),
        question.Required,
        [.. (question.Options ?? []).Select(option => new FormOption(option.Id, option.Label?.Trim()!))],
        question.ScaleMin,
        question.ScaleMax,
        NullIfBlank(question.MinLabel),
        NullIfBlank(question.MaxLabel),
        question.ShowIf is { } condition ? new QuestionCondition(condition.QuestionId, condition.AnyOf ?? []) : null);

    private static FormQuestionDto ToDto(FormQuestion question) => new(
        question.Id,
        question.Type,
        question.Text,
        question.HelpText,
        question.Required,
        question.Options.Count == 0 ? null : [.. question.Options.Select(option => new FormOptionDto(option.Id, option.Label))],
        question.ScaleMin,
        question.ScaleMax,
        question.MinLabel,
        question.MaxLabel,
        question.ShowIf is { } condition ? new QuestionConditionDto(condition.QuestionId, condition.AnyOf) : null);

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
