using Klf.Application.DTOs.Feedback;
using Klf.Application.Services.Feedback;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class FeedbackFormServiceTests
{
    private readonly InMemoryFeedbackStore _store = new();
    private readonly FeedbackFormService _service;

    public FeedbackFormServiceTests()
    {
        _service = new FeedbackFormService(_store, _store);
    }

    [Fact]
    public async Task Create_stores_trimmed_form_and_round_trips_the_definition()
    {
        var response = await _service.CreateAsync(new CreateFeedbackFormRequest("  Padrão  ", " ", Definition()), TestContext.Current.CancellationToken);

        var form = Assert.Single(_store.Forms);
        Assert.Equal("Padrão", form.Title);
        Assert.Null(form.Description);
        Assert.Equal("Recomendaria?", response.Definition.Sections[0].Questions[1].Text);
        Assert.Equal(["0", "1"], response.Definition.Sections[0].Questions[2].ShowIf!.AnyOf);
    }

    [Fact]
    public async Task Update_and_delete_throw_not_found_when_template_does_not_exist()
    {
        var token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.CreateVersion7(), new UpdateFeedbackFormRequest("X", null, Definition()), token));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.CreateVersion7(), token));
    }

    [Fact]
    public async Task List_returns_counts_of_sections_and_questions()
    {
        await _service.CreateAsync(new CreateFeedbackFormRequest("Padrão", null, Definition()), TestContext.Current.CancellationToken);

        var item = Assert.Single(await _service.ListAsync(TestContext.Current.CancellationToken));

        Assert.Equal(1, item.SectionCount);
        Assert.Equal(3, item.QuestionCount);
    }

    internal static FormDefinitionDto Definition() => new(
    [
        new FormSectionDto("klf", " Treinamento ", null, FeedbackTopic.Training,
        [
            new FormQuestionDto("nota", FeedbackQuestionType.Scale, "Nota?", null, true, null, 1, 5, "Ruim", "Ótimo", null),
            new FormQuestionDto("nps", FeedbackQuestionType.Nps, " Recomendaria? ", null, true, null, null, null, null, null, null),
            new FormQuestionDto("porque", FeedbackQuestionType.LongText, "Por quê?", null, false, null, null, null, null, null, new QuestionConditionDto("nps", ["0", "1"])),
        ]),
    ]);
}
