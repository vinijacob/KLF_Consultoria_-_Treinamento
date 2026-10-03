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

    /// <summary>Returns the testimonial with the given id, tracked for changes, or <see langword="null"/>.</summary>
    Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Marks a new testimonial to be inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Testimonial testimonial);

    /// <summary>Marks a testimonial to be (soft) deleted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(Testimonial testimonial);
}
