using Klf.Application.DTOs.Feedback;
using Klf.Application.Tests.Services;
using Klf.Application.Validators.Feedback;
using Klf.Domain.Enums;

namespace Klf.Application.Tests.Validators;

public sealed class FeedbackValidatorTests
{
    private static readonly DateTimeOffset Opens = new(2026, 10, 7, 8, 0, 0, TimeSpan.FromHours(-4));

    [Fact]
    public void Form_request_is_valid_with_sample_definition()
    {
        Assert.True(new CreateFeedbackFormRequestValidator().Validate(new CreateFeedbackFormRequest("Padrão", null, FeedbackFormServiceTests.Definition())).IsValid);
    }

    [Fact]
    public void Form_request_reports_structure_problems_with_full_path()
    {
        var broken = new FormDefinitionDto(
        [
            new FormSectionDto("s", "Seção", null, FeedbackTopic.Training,
                [new FormQuestionDto("q", FeedbackQuestionType.SingleChoice, "Qual?", null, true, [new FormOptionDto("a", "A")], null, null, null, null, null)]),
        ]);

        var result = new CreateFeedbackFormRequestValidator().Validate(new CreateFeedbackFormRequest("Padrão", null, broken));

        Assert.Contains(result.Errors, e => e.PropertyName == "Definition.Sections[0].Questions[0].Options");
    }

    [Fact]
    public void Form_request_requires_definition()
    {
        var result = new CreateFeedbackFormRequestValidator().Validate(new CreateFeedbackFormRequest("Padrão", null, null!));

        Assert.Contains(result.Errors, e => e.PropertyName == "Definition" && e.ErrorMessage == "Informe as perguntas do formulário.");
    }

    [Fact]
    public void Session_request_rejects_closing_before_opening_and_period_over_90_days()
    {
        var validator = new CreateFeedbackSessionRequestValidator();

        var backwards = validator.Validate(new CreateFeedbackSessionRequest(Guid.CreateVersion7(), "T", Opens, Opens.AddHours(-1), null, null, null));
        var tooLong = validator.Validate(new CreateFeedbackSessionRequest(Guid.CreateVersion7(), "T", Opens, Opens.AddDays(91), null, null, null));
        var ok = validator.Validate(new CreateFeedbackSessionRequest(Guid.CreateVersion7(), "T", Opens, Opens.AddHours(4), 30, null, null));

        Assert.Contains(backwards.Errors, e => e.PropertyName == "ClosesAt");
        Assert.Contains(tooLong.Errors, e => e.PropertyName == "ClosesAt");
        Assert.True(ok.IsValid);
    }

    [Fact]
    public void Session_request_rejects_empty_form_and_bad_limit()
    {
        var result = new CreateFeedbackSessionRequestValidator().Validate(
            new CreateFeedbackSessionRequest(Guid.Empty, "", Opens, Opens.AddHours(1), 0, null, null));

        Assert.Contains(result.Errors, e => e.PropertyName == "FormId");
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        Assert.Contains(result.Errors, e => e.PropertyName == "MaxResponses");
    }

    [Fact]
    public void Submit_request_rejects_empty_list_and_oversized_text()
    {
        var validator = new SubmitFeedbackRequestValidator();

        Assert.False(validator.Validate(new SubmitFeedbackRequest([])).IsValid);
        Assert.False(validator.Validate(new SubmitFeedbackRequest([new FeedbackAnswerDto("q", new string('a', 3001), null, null)])).IsValid);
        Assert.True(validator.Validate(new SubmitFeedbackRequest([new FeedbackAnswerDto("q", "ok", null, null)])).IsValid);
    }
}
