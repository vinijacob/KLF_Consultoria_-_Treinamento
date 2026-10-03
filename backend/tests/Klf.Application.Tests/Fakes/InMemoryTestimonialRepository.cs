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

    public Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Testimonials.SingleOrDefault(x => x.Id == id));

    public void Add(Testimonial testimonial) => Testimonials.Add(testimonial);

    public void Remove(Testimonial testimonial) => Testimonials.Remove(testimonial);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(++SaveCount);
}
