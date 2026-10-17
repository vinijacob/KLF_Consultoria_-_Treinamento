using Klf.Application.Interfaces.Repositories;
using Klf.Application.Services.Posts;
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

    public async Task<IReadOnlyList<Testimonial>> ListLatestVisibleAsync(int count, CancellationToken cancellationToken) =>
        await context.Testimonials
            .AsNoTracking()
            .Where(Testimonial.IsVisible)
            .OrderByDescending(testimonial => testimonial.CreatedAt)
            .ThenByDescending(testimonial => testimonial.Id)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Testimonial>> ListInOrderAsync(CancellationToken cancellationToken) =>
        await context.Testimonials
            .OrderBy(testimonial => testimonial.DisplayOrder)
            .ThenBy(testimonial => testimonial.AuthorName)
            .ToListAsync(cancellationToken);

    public Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Testimonials.FirstOrDefaultAsync(testimonial => testimonial.Id == id, cancellationToken);

    public void Add(Testimonial testimonial) => context.Testimonials.Add(testimonial);

    public async Task<IReadOnlyList<Testimonial>> SearchByAuthorAsync(string name, CancellationToken cancellationToken)
    {
        var wanted = TaxonomyText.ToSlug(name);

        if (wanted.Length == 0)
        {
            return [];
        }

        var all = await context.Testimonials.IgnoreQueryFilters().AsNoTracking().ToListAsync(cancellationToken);

        return [.. all.Where(testimonial => TaxonomyText.ToSlug(testimonial.AuthorName).Contains(wanted, StringComparison.Ordinal))];
    }

    public Task<Testimonial?> GetIncludingDeletedAsync(Guid id, CancellationToken cancellationToken) =>
        context.Testimonials.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(testimonial => testimonial.Id == id, cancellationToken);

    public async Task EraseAsync(Guid id, CancellationToken cancellationToken)
    {
        var position = await context.Testimonials
            .Where(testimonial => testimonial.Id == id)
            .Select(testimonial => (int?)testimonial.DisplayOrder)
            .FirstOrDefaultAsync(cancellationToken);

        await context.Testimonials.IgnoreQueryFilters().Where(testimonial => testimonial.Id == id).ExecuteDeleteAsync(cancellationToken);

        if (position is { } removed)
        {
            await context.Testimonials
                .Where(testimonial => testimonial.DisplayOrder > removed)
                .ExecuteUpdateAsync(setters => setters.SetProperty(testimonial => testimonial.DisplayOrder, testimonial => testimonial.DisplayOrder - 1), cancellationToken);
        }
    }

    public void Remove(Testimonial testimonial) => context.Testimonials.Remove(testimonial);
}
