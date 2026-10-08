using Klf.Application.DTOs.Feedback;

namespace Klf.Application.Services.Feedback;

/// <summary>Manages the feedback form templates.</summary>
public interface IFeedbackFormService
{
    /// <summary>Lists every template ordered by title.</summary>
    Task<IReadOnlyList<FeedbackFormListItemResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns one template with its questions.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The template does not exist.</exception>
    Task<FeedbackFormResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Creates a template.</summary>
    Task<FeedbackFormResponse> CreateAsync(CreateFeedbackFormRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces a template. Sessions already created keep their own copy of the questions.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The template does not exist.</exception>
    Task<FeedbackFormResponse> UpdateAsync(Guid id, UpdateFeedbackFormRequest request, CancellationToken cancellationToken);

    /// <summary>Soft-deletes a template. Sessions created from it are not affected.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The template does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
