using Klf.Domain.Entities;

namespace Klf.Application.Interfaces.Repositories;

/// <summary>Data access for <see cref="Testimonial"/>. Soft-deleted testimonials are never returned.</summary>
public interface ITestimonialRepository
{
    /// <summary>
    /// Returns the testimonials ordered by <see cref="Testimonial.DisplayOrder"/> (then author), read-only.
    /// </summary>
    /// <param name="onlyVisible">When <see langword="true"/>, keeps only those matching <see cref="Testimonial.IsVisible"/> (public site).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<Testimonial>> ListAsync(bool onlyVisible, CancellationToken cancellationToken);

    /// <summary>Returns up to <paramref name="count"/> visible testimonials, the most recently added first, read-only.</summary>
    Task<IReadOnlyList<Testimonial>> ListLatestVisibleAsync(int count, CancellationToken cancellationToken);

    /// <summary>Returns the testimonial with the given id, tracked for changes, or <see langword="null"/>.</summary>
    Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Returns every testimonial in the order of the panel list (position, then name), tracked for changes, so their
    /// positions can be renumbered (see <see cref="Klf.Domain.Common.DisplayOrdering"/>).
    /// </summary>
    Task<IReadOnlyList<Testimonial>> ListInOrderAsync(CancellationToken cancellationToken);

    /// <summary>Marks a new testimonial to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Testimonial testimonial);

    /// <summary>Testimonials whose author name contains <paramref name="name"/> (case and accents ignored), deleted ones included. Read-only.</summary>
    Task<IReadOnlyList<Testimonial>> SearchByAuthorAsync(string name, CancellationToken cancellationToken);

    /// <summary>Returns the testimonial even if it was (soft) deleted, read-only, or <see langword="null"/>.</summary>
    Task<Testimonial?> GetIncludingDeletedAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the testimonial for real, right away (no soft delete): a data subject asked for it. The testimonials after it
    /// move up one position, so the list keeps no gap.
    /// </summary>
    Task EraseAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Marks a testimonial to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(Testimonial testimonial);
}
