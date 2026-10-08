namespace Klf.Domain.Enums;

/// <summary>Whether a feedback session accepts responses.</summary>
public enum FeedbackSessionStatus
{
    /// <summary>Before the opening time.</summary>
    Scheduled,

    /// <summary>Accepting responses.</summary>
    Open,

    /// <summary>After the closing time, closed by hand, or the response limit was reached.</summary>
    Closed,
}
