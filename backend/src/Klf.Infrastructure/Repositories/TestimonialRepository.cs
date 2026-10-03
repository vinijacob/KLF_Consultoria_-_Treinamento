using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Repositories;

internal sealed class TestimonialRepository(AppDbContext context) : ITestimonialRepository
{
    public async Task<IReadOnlyList<Testimonial>> ListAsync(bool onlyVisible, CancellationToken cancellationToken)
    {
        var query = context.Testimonials.AsNoTracking();

        if (onlyVisible)
        {
            query = query.Where(Testimonial.IsVisible);
        }

        return await query
            .OrderBy(testimonial => testimonial.DisplayOrder)
            .ThenBy(testimonial => testimonial.AuthorName)
            .ToListAsync(cancellationToken);
    }

    public Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Testimonials.FirstOrDefaultAsync(testimonial => testimonial.Id == id, cancellationToken);

    public void Add(Testimonial testimonial) => context.Testimonials.Add(testimonial);

    public void Remove(Testimonial testimonial) => context.Testimonials.Remove(testimonial);
}
