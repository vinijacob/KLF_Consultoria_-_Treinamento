using Klf.Application.DTOs.Feedback;
using Klf.Application.Services.Feedback;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class PublicFeedbackServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 2, 30, 0, TimeSpan.Zero);

    private readonly InMemoryFeedbackStore _store = new();
    private readonly PublicFeedbackService _service;

    public PublicFeedbackServiceTests()
    {
        _service = new PublicFeedbackService(_store, _store, _store, new MutableTimeProvider(Now));
    }

    [Fact]
    public async Task Get_form_returns_questions_only_when_open_and_not_answered()
    {
        var open = _store.SeedSession(Guid.CreateVersion7(), Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1));
        var scheduled = _store.SeedSession(Guid.CreateVersion7(), Now.UtcDateTime.AddHours(1), Now.UtcDateTime.AddHours(2));
        var token = TestContext.Current.CancellationToken;

        var fresh = await _service.GetFormAsync(open.PublicCode, alreadyAnswered: false, token);
        var answered = await _service.GetFormAsync(open.PublicCode, alreadyAnswered: true, token);
        var early = await _service.GetFormAsync(scheduled.PublicCode, alreadyAnswered: false, token);

        Assert.NotNull(fresh.Definition);
        Assert.Null(answered.Definition);
        Assert.True(answered.AlreadyAnswered);
        Assert.Equal(FeedbackSessionStatus.Scheduled, early.Status);
        Assert.Null(early.Definition);
    }

    [Fact]
    public async Task Get_form_throws_not_found_for_unknown_or_malformed_code()
    {
        var token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetFormAsync("AAAAAAAAAAAAAAAAAAAAAA", false, token));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetFormAsync("curto", false, token));
    }

    [Fact]
    public async Task Submit_stores_only_the_local_date_and_clean_answers()
    {
        var session = _store.SeedSession(Guid.CreateVersion7(), Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1));

        var submission = await _service.SubmitAsync(session.PublicCode, Request(), alreadyAnswered: false, TestContext.Current.CancellationToken);

        var stored = Assert.Single(_store.Responses);
        Assert.Equal(new DateOnly(2026, 10, 7), stored.SubmittedOn);
        Assert.Equal(session.Id, stored.SessionId);
        Assert.Equal(3, stored.Answers.Count);
        Assert.Equal(session.ClosesAt.AddDays(1), submission.RememberUntil);
    }

    [Fact]
    public async Task Submit_throws_conflict_when_already_answered_scheduled_closed_or_full()
    {
        var open = _store.SeedSession(Guid.CreateVersion7(), Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1));
        var scheduled = _store.SeedSession(Guid.CreateVersion7(), Now.UtcDateTime.AddHours(1), Now.UtcDateTime.AddHours(2));
        var closed = _store.SeedSession(Guid.CreateVersion7(), Now.UtcDateTime.AddHours(-3), Now.UtcDateTime.AddHours(-1));
        var full = _store.SeedSession(Guid.CreateVersion7(), Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1), maxResponses: 1);
        _store.SeedResponses(full, 9);
        var token = TestContext.Current.CancellationToken;

        var again = await Assert.ThrowsAsync<ConflictException>(() => _service.SubmitAsync(open.PublicCode, Request(), alreadyAnswered: true, token));
        var early = await Assert.ThrowsAsync<ConflictException>(() => _service.SubmitAsync(scheduled.PublicCode, Request(), false, token));
        var late = await Assert.ThrowsAsync<ConflictException>(() => _service.SubmitAsync(closed.PublicCode, Request(), false, token));
        await Assert.ThrowsAsync<ConflictException>(() => _service.SubmitAsync(full.PublicCode, Request(), false, token));

        Assert.Equal("Você já respondeu esta avaliação. Obrigado!", again.Message);
        Assert.Equal("Esta avaliação ainda não começou.", early.Message);
        Assert.Equal("Esta avaliação já foi encerrada.", late.Message);
        Assert.Single(_store.Responses);
    }

    [Fact]
    public async Task Submit_throws_validation_when_answers_do_not_match_the_form()
    {
        var session = _store.SeedSession(Guid.CreateVersion7(), Now.UtcDateTime.AddHours(-1), Now.UtcDateTime.AddHours(1));

        var error = await Assert.ThrowsAsync<ValidationException>(() => _service.SubmitAsync(
            session.PublicCode,
            new SubmitFeedbackRequest([new FeedbackAnswerDto("nota", null, 7, null)]),
            false,
            TestContext.Current.CancellationToken));

        Assert.Contains("Answers.bom", error.Errors.Keys);
        Assert.Empty(_store.Responses);
    }

    private static SubmitFeedbackRequest Request() => new(
    [
        new FeedbackAnswerDto("bom", "Equipe unida", null, null),
        new FeedbackAnswerDto("nota", null, 5, null),
        new FeedbackAnswerDto("nps", null, 10, null),
        new FeedbackAnswerDto("porque", "ignorado: nps alto", null, null),
    ]);
}
