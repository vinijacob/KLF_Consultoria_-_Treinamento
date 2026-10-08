namespace Klf.Application.Services.Feedback;

/// <summary>Who is calling a feedback operation: admins see every session, instructors only the ones they manage.</summary>
/// <param name="UserId">The signed-in user.</param>
/// <param name="IsAdmin">Whether the user has the Admin role.</param>
public sealed record FeedbackActor(Guid UserId, bool IsAdmin);
