using Klf.Application.DTOs.Feedback;
using Klf.Application.Services.Feedback;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class FeedbackSessionServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 14, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.CreateVersion7();

    private readonly InMemoryFeedbackStore _store = new();
    private readonly InMemoryClientRepository _clients = new();
    private readonly InMemoryServiceRepository _services = new();
    private readonly FakePosterRenderer _renderer = new();
    private readonly FeedbackSessionService _service;

    public FeedbackSessionServiceTests()
    {
        _service = new FeedbackSessionService(
            _store, _store, _store, _clients, _services, _store, new FakeFrontendLinks(), _renderer, new MutableTimeProvider(Now));
    }

    [Fact]
    public async Task Create_copies_template_questions_and_later_template_edits_do_not_change_the_session()
    {
        var form = new FeedbackForm("Padrão", "Desc", FeedbackSamples.Definition());
        _store.Forms.Add(form);

        var response = await _service.CreateAsync(UserId, Request(form.Id), TestContext.Current.CancellationToken);
        form.Update("Mudou", null, new FormDefinition([new FormSection("x", "X", null, FeedbackTopic.Training, [FeedbackSamples.Question("q", FeedbackQuestionType.ShortText)])]));

        var session = Assert.Single(_store.Sessions);
        Assert.Equal(UserId, session.OwnerId);
        Assert.Equal("Padrão", session.FormTitle);
        Assert.Equal(2, session.Definition.Sections.Count);
        Assert.Equal($"https://klf.test/avaliar/{session.PublicCode}", response.PublicUrl);
        Assert.Equal(FeedbackSessionStatus.Open, response.Status);
    }

    [Fact]
    public async Task Create_throws_validation_when_template_client_or_service_does_not_exist()
    {
        var form = new FeedbackForm("Padrão", null, FeedbackSamples.Definition());
        _store.Forms.Add(form);
        var token = TestContext.Current.CancellationToken;

        var noForm = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(UserId, Request(Guid.CreateVersion7()), token));
        var noClient = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(UserId, Request(form.Id) with { ClientId = Guid.CreateVersion7() }, token));
        var noService = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(UserId, Request(form.Id) with { ServiceId = Guid.CreateVersion7() }, token));

        Assert.Contains("FormId", noForm.Errors.Keys);
        Assert.Contains("ClientId", noClient.Errors.Keys);
        Assert.Contains("ServiceId", noService.Errors.Keys);
        Assert.Empty(_store.Sessions);
    }

    [Fact]
    public async Task List_reports_response_count_and_closed_status_when_limit_is_reached()
    {
        var session = _store.SeedSession(UserId, Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1), maxResponses: 2);
        _store.SeedResponses(session, 9, 10);

        var list = await _service.ListAsync(new FeedbackSessionListRequest(), TestContext.Current.CancellationToken);

        var item = Assert.Single(list.Items);
        Assert.Equal(2, item.ResponseCount);
        Assert.Equal(FeedbackSessionStatus.Closed, item.Status);
    }

    [Fact]
    public async Task Replace_form_works_without_responses_and_throws_conflict_after_the_first()
    {
        var session = _store.SeedSession(UserId, Now.UtcDateTime, Now.UtcDateTime.AddHours(2));
        var request = new ReplaceSessionFormRequest("Só desta turma", null, new FormDefinitionDto(
            [new FormSectionDto("s", "Seção", null, FeedbackTopic.Training, [new FormQuestionDto("q", FeedbackQuestionType.ShortText, "Algo?", null, true, null, null, null, null, null, null)])]));
        var token = TestContext.Current.CancellationToken;

        var replaced = await _service.ReplaceFormAsync(session.Id, request, token);
        _store.SeedResponses(session, 9);

        Assert.Equal("Só desta turma", replaced.FormTitle);
        await Assert.ThrowsAsync<ConflictException>(() => _service.ReplaceFormAsync(session.Id, request, token));
    }

    [Fact]
    public async Task Close_and_reopen_change_status()
    {
        var session = _store.SeedSession(UserId, Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1));
        var token = TestContext.Current.CancellationToken;

        var closed = await _service.CloseAsync(session.Id, token);
        var reopened = await _service.ReopenAsync(session.Id, token);

        Assert.Equal(FeedbackSessionStatus.Closed, closed.Status);
        Assert.Equal(FeedbackSessionStatus.Open, reopened.Status);
    }

    [Fact]
    public async Task Results_hide_everything_below_three_responses()
    {
        var session = _store.SeedSession(UserId, Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1));
        _store.SeedResponses(session, 10, 2);

        var results = await _service.GetResultsAsync(session.Id, TestContext.Current.CancellationToken);

        Assert.False(results.HasEnoughResponses);
        Assert.Equal(2, results.ResponseCount);
        Assert.Empty(results.Sections);
        Assert.Null(results.Nps);
    }

    [Fact]
    public async Task Results_compute_nps_averages_distribution_and_shuffled_texts()
    {
        var session = _store.SeedSession(UserId, Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1));
        _store.SeedResponses(session, 10, 9, 8, 6);

        var results = await _service.GetResultsAsync(session.Id, TestContext.Current.CancellationToken);

        Assert.True(results.HasEnoughResponses);
        Assert.Equal(new NpsResult(25, 2, 1, 1, 4), results.Nps);
        var training = results.Sections.Single(s => s.Topic == FeedbackTopic.Training);
        var nota = training.Questions.Single(q => q.Id == "nota");
        Assert.Equal(5.0, nota.Average);
        Assert.Equal([0, 0, 0, 0, 4], nota.Distribution!.Select(d => d.Count));
        var nps = training.Questions.Single(q => q.Id == "nps");
        Assert.Equal(11, nps.Distribution!.Count);
        var bom = results.Sections.Single(s => s.Topic == FeedbackTopic.Company).Questions.Single(q => q.Id == "bom");
        Assert.Equal(["Texto 10", "Texto 6", "Texto 8", "Texto 9"], bom.Texts!.Order());
    }

    [Fact]
    public async Task Results_hide_a_question_answered_by_fewer_than_three_people()
    {
        var session = _store.SeedSession(UserId, Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1));
        _store.SeedResponses(session, 10, 9, 10);
        _store.Responses.Add(new FeedbackResponse(session.Id, new DateOnly(2026, 10, 7),
            [.. FeedbackSamples.ValidAnswers(nps: 2), new FeedbackAnswer("porque", "Só eu escrevi isto", null, null)]));

        var results = await _service.GetResultsAsync(session.Id, TestContext.Current.CancellationToken);

        var porque = results.Sections.SelectMany(s => s.Questions).Single(q => q.Id == "porque");
        Assert.True(porque.IsHidden);
        Assert.Equal(1, porque.AnswerCount);
        Assert.Null(porque.Texts);
    }

    [Fact]
    public async Task Summary_joins_sessions_of_the_period_and_skips_small_ones_in_nps()
    {
        var day = Now.UtcDateTime;
        var big = _store.SeedSession(UserId, day, day.AddHours(2), title: "Grande");
        var small = _store.SeedSession(UserId, day, day.AddHours(2), title: "Pequena");
        var old = _store.SeedSession(UserId, day.AddDays(-40), day.AddDays(-39), title: "Antiga");
        _store.SeedResponses(big, 10, 10, 0);
        _store.SeedResponses(small, 0, 0);
        _store.SeedResponses(old, 0, 0, 0);

        var summary = await _service.GetSummaryAsync(
            new FeedbackSummaryRequest { From = new DateOnly(2026, 10, 1), To = new DateOnly(2026, 10, 31) },
            TestContext.Current.CancellationToken);

        Assert.Equal(2, summary.SessionCount);
        Assert.Equal(5, summary.ResponseCount);
        Assert.Equal(new NpsResult(33, 2, 0, 1, 3), summary.Nps);
        Assert.Null(summary.Sessions.Single(s => s.Id == small.Id).Nps);
    }

    [Fact]
    public async Task Poster_uses_local_times_and_public_url()
    {
        var session = _store.SeedSession(UserId, new DateTime(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 7, 16, 0, 0, DateTimeKind.Utc));

        var file = await _service.GetPosterAsync(session.Id, TestContext.Current.CancellationToken);

        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal(new DateTime(2026, 10, 7, 8, 0, 0), _renderer.LastPoster!.OpensAt);
        Assert.Equal($"https://klf.test/avaliar/{session.PublicCode}", _renderer.LastPoster.Url.AbsoluteUri);
    }

    private static CreateFeedbackSessionRequest Request(Guid formId) =>
        new(formId, "Loja Centro — manhã", Now.AddHours(-1), Now.AddHours(3), null, null, null);
}
