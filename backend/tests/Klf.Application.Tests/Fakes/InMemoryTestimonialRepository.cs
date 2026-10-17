using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Fakes;

internal sealed class InMemoryTestimonialRepository : ITestimonialRepository, IUnitOfWork
{
    private static readonly Func<Testimonial, bool> Compiled = Testimonial.IsVisible.Compile();
    public List<Testimonial> Testimonials { get; } = [];

    public int SaveCount { get; private set; }
    public Task<IReadOnlyList<Testimonial>> ListAsync(bool onlyVisible, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Testimonial>>([.. Testimonials.Where(x => !onlyVisible || Compiled(x)).OrderBy(x => x.DisplayOrder).ThenBy(x => x.AuthorName)]);

    public Task<IReadOnlyList<Testimonial>> ListLatestVisibleAsync(int count, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Testimonial>>([.. Testimonials.Where(Compiled).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Take(count)]);

    public Task<IReadOnlyList<Testimonial>> ListInOrderAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Testimonial>>([.. Testimonials.OrderBy(x => x.DisplayOrder).ThenBy(x => x.AuthorName, StringComparer.Ordinal)]);

    public Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Testimonials.SingleOrDefault(x => x.Id == id));

    public void Add(Testimonial testimonial) => Testimonials.Add(testimonial);

    public void Remove(Testimonial testimonial) => Testimonials.Remove(testimonial);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(++SaveCount);

    public Task<IReadOnlyList<Testimonial>> SearchByAuthorAsync(string name, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Testimonial>>([.. Testimonials.Where(x => Klf.Application.Services.Posts.TaxonomyText.ToSlug(x.AuthorName)
            .Contains(Klf.Application.Services.Posts.TaxonomyText.ToSlug(name), StringComparison.Ordinal))]);

    public Task<Testimonial?> GetIncludingDeletedAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Testimonials.SingleOrDefault(x => x.Id == id));

    public Task EraseAsync(Guid id, CancellationToken cancellationToken)
    {
        var erased = Testimonials.SingleOrDefault(x => x.Id == id);
        Testimonials.RemoveAll(x => x.Id == id);

        foreach (var after in Testimonials.Where(x => erased is not null && x.DisplayOrder > erased.DisplayOrder))
        {
            after.MoveTo(after.DisplayOrder - 1);
        }

        return Task.CompletedTask;
    }
}
