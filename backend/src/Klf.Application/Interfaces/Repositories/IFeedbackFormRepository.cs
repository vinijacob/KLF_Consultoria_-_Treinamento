using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="FeedbackForm"/> templates. Soft-deleted templates are never returned.</summary>
public interface IFeedbackFormRepository
{
    /// <summary>Returns every template ordered by title, read-only.</summary>
    Task<IReadOnlyList<FeedbackForm>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns the template with the given id, tracked for changes, or <see langword="null"/>.</summary>
    Task<FeedbackForm?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Marks a new template to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(FeedbackForm form);

    /// <summary>Marks a template to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(FeedbackForm form);
}
