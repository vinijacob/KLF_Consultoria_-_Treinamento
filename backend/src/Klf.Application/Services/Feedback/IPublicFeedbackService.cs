using Klf.Application.DTOs.Feedback;

namespace Klf.Application.Services.Feedback;

/// <summary>When a browser should stop being told it may answer again.</summary>
/// <param name="SessionId">The session answered.</param>
/// <param name="RememberUntil">Until when the "already answered" mark is kept (UTC).</param>
public sealed record FeedbackSubmission(Guid SessionId, DateTime RememberUntil);

/// <summary>The anonymous side of feedback: showing the form of a session and receiving responses.</summary>
public interface IPublicFeedbackService
{
    /// <summary>Returns what the feedback page needs; the questions only while the session is open and this browser has not answered.</summary>
    /// <param name="publicCode">Code from the address.</param>
    /// <param name="alreadyAnswered">Whether this browser carries a valid "already answered" mark for the session.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="Domain.Exceptions.NotFoundException">No session has this code.</exception>
    Task<PublicFeedbackFormResponse> GetFormAsync(string publicCode, bool alreadyAnswered, CancellationToken cancellationToken);

    /// <summary>Stores an anonymous response with only the local date.</summary>
    /// <param name="publicCode">Code from the address.</param>
    /// <param name="request">The answers.</param>
    /// <param name="alreadyAnswered">Whether this browser carries a valid "already answered" mark for the session.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="Domain.Exceptions.NotFoundException">No session has this code.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">Not open, or already answered from this browser.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">The answers do not match the form.</exception>
    Task<FeedbackSubmission> SubmitAsync(string publicCode, SubmitFeedbackRequest request, bool alreadyAnswered, CancellationToken cancellationToken);
}
