using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Tests.Entities;

public sealed class FeedbackSessionTests
{
    private static readonly DateTime Opens = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Closes = Opens.AddHours(4);

    [Fact]
    public void Public_code_is_22_url_safe_characters_and_unique()
    {
        var first = Create();
        var second = Create();

        Assert.Equal(FeedbackSession.PublicCodeLength, first.PublicCode.Length);
        Assert.Matches("^[A-Za-z0-9_-]{22}$", first.PublicCode);
        Assert.NotEqual(first.PublicCode, second.PublicCode);
    }

    [Fact]
    public void Status_follows_period_limit_and_manual_close()
    {
        var session = Create(maxResponses: 2);

        Assert.Equal(FeedbackSessionStatus.Scheduled, session.GetStatus(Opens.AddMinutes(-1), 0));
        Assert.Equal(FeedbackSessionStatus.Open, session.GetStatus(Opens, 0));
        Assert.Equal(FeedbackSessionStatus.Closed, session.GetStatus(Opens.AddMinutes(1), 2));
        Assert.Equal(FeedbackSessionStatus.Closed, session.GetStatus(Closes, 0));

        session.Close(Opens.AddMinutes(5));
        Assert.Equal(FeedbackSessionStatus.Closed, session.GetStatus(Opens.AddMinutes(10), 0));

        session.Reopen();
        Assert.Equal(FeedbackSessionStatus.Open, session.GetStatus(Opens.AddMinutes(10), 0));
    }

    [Fact]
    public void Creation_throws_when_closing_is_not_after_opening()
    {
        var error = Assert.Throws<ValidationException>(() =>
            new FeedbackSession(Guid.CreateVersion7(), "T", null, "F", null, FeedbackSamples.Definition(), Opens, Opens, null, null, null));

        Assert.Contains("ClosesAt", error.Errors.Keys);
    }

    [Fact]
    public void Replace_form_throws_conflict_once_there_are_responses()
    {
        var session = Create();

        Assert.Throws<ConflictException>(() => session.ReplaceForm(null, "Novo", null, FeedbackSamples.Definition(), responseCount: 1));
        session.ReplaceForm(null, "Novo", null, FeedbackSamples.Definition(), responseCount: 0);
        Assert.Equal("Novo", session.FormTitle);
    }

    [Fact]
    public void Response_id_is_random_v4_and_carries_no_time()
    {
        var response = new FeedbackResponse(Guid.CreateVersion7(), new DateOnly(2026, 10, 7), FeedbackSamples.ValidAnswers());

        Assert.Equal(4, response.Id.Version);
        Assert.DoesNotContain(typeof(FeedbackResponse).GetProperties(), p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));
    }

    private static FeedbackSession Create(int? maxResponses = null) =>
        new(Guid.CreateVersion7(), "Loja Centro", null, "Avaliação", null, FeedbackSamples.Definition(), Opens, Closes, maxResponses, null, null);
}
