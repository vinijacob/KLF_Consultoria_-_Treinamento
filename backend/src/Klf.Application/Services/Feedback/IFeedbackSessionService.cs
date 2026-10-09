using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Feedback;

namespace Klf.Application.Services.Feedback;

/// <summary>Manages feedback sessions (turmas) and their results.</summary>
public interface IFeedbackSessionService
{
    /// <summary>Lists the sessions, newest opening first.</summary>
    Task<PagedResponse<FeedbackSessionListItemResponse>> ListAsync(FeedbackSessionListRequest request, CancellationToken cancellationToken);

    /// <summary>Returns one session with its frozen questions.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The session does not exist.</exception>
    Task<FeedbackSessionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Opens a session copying the questions of a template. The user is recorded as its creator.</summary>
    /// <exception cref="Domain.Exceptions.ValidationException">Unknown template, client or service, or invalid period.</exception>
    Task<FeedbackSessionResponse> CreateAsync(Guid userId, CreateFeedbackSessionRequest request, CancellationToken cancellationToken);

    /// <summary>Changes the scheduling and links of a session.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The session does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">Unknown client or service, or invalid period.</exception>
    Task<FeedbackSessionResponse> UpdateAsync(Guid id, UpdateFeedbackSessionRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces the questions of a session that has no responses yet.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The session does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">The session already has responses.</exception>
    Task<FeedbackSessionResponse> ReplaceFormAsync(Guid id, ReplaceSessionFormRequest request, CancellationToken cancellationToken);

    /// <summary>Stops accepting responses now.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The session does not exist.</exception>
    Task<FeedbackSessionResponse> CloseAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Undoes a manual close; the session follows its period and limit again.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The session does not exist.</exception>
    Task<FeedbackSessionResponse> ReopenAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Soft-deletes a session; its responses stay in the database but leave the panel.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The session does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Aggregated results of a session (hidden below the minimum of responses).</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The session does not exist.</exception>
    Task<FeedbackSessionResultsResponse> GetResultsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Totals and NPS of the sessions opened in a period.</summary>
    Task<FeedbackSummaryResponse> GetSummaryAsync(FeedbackSummaryRequest request, CancellationToken cancellationToken);

    /// <summary>PNG with the QR Code of the public address.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The session does not exist.</exception>
    Task<FileDownload> GetQrCodeAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Printable A4 PDF with the QR Code, the session name, the period and the anonymity notice.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The session does not exist.</exception>
    Task<FileDownload> GetPosterAsync(Guid id, CancellationToken cancellationToken);
}
