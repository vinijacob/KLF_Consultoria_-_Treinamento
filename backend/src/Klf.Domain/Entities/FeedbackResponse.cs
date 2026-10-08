namespace Klf.Domain.Entities;

/// <summary>
/// One anonymous response to a <see cref="FeedbackSession"/>. On purpose it does not inherit from <see cref="Common.Entity"/>:
/// its id is a random UUID v4 (a v7 would embed the exact time) and it has no creation timestamp, only the local date.
/// It never stores IP, user agent, location, exact time or user.
/// </summary>
public sealed class FeedbackResponse
{
    /// <summary>Records a response.</summary>
    /// <param name="sessionId">The session answered.</param>
    /// <param name="submittedOn">Local calendar day of the response (no time).</param>
    /// <param name="answers">Answers already checked by <see cref="FormDefinition.NormalizeAnswers"/>.</param>
    public FeedbackResponse(Guid sessionId, DateOnly submittedOn, IReadOnlyList<FeedbackAnswer> answers)
    {
        Id = Guid.NewGuid();
        SessionId = sessionId;
        SubmittedOn = submittedOn;
        Answers = answers;
    }

    private FeedbackResponse()
    {
        Answers = null!;
    }

    /// <summary>Random identifier (UUID v4, carries no time).</summary>
    public Guid Id { get; private set; }

    /// <summary>The session answered.</summary>
    public Guid SessionId { get; private set; }

    /// <summary>Local calendar day of the response (no time, on purpose).</summary>
    public DateOnly SubmittedOn { get; private set; }

    /// <summary>The answers.</summary>
    public IReadOnlyList<FeedbackAnswer> Answers { get; private set; }
}
