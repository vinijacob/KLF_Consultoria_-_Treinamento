using Klf.Application.DTOs.Posts;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Posts;

internal sealed class CategoryService(ICategoryRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider) : ICategoryService
{
    public async Task<IReadOnlyList<PublicTaxonomyItemResponse>> ListPublicAsync(CancellationToken cancellationToken) =>
        [.. (await repository.ListAsync(timeProvider.GetUtcNow().UtcDateTime, cancellationToken))
            .Select(usage => new PublicTaxonomyItemResponse(usage.Item.Name, usage.Item.Slug, usage.PostCount))];

    public async Task<IReadOnlyList<TaxonomyItemResponse>> ListAsync(CancellationToken cancellationToken) =>
        [.. (await repository.ListAsync(visibleAt: null, cancellationToken))
            .Select(usage => new TaxonomyItemResponse(usage.Item.Id, usage.Item.Name, usage.Item.Slug, usage.PostCount))];

    public async Task<TaxonomyItemResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var (name, slug) = await NameAndFreeSlugAsync(request.Name, excludingId: null, cancellationToken);
        var category = new Category(name, slug);

        repository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaxonomyItemResponse(category.Id, category.Name, category.Slug, 0);
    }

    public async Task<TaxonomyItemResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await GetOrThrowAsync(id, cancellationToken);
        var (name, slug) = await NameAndFreeSlugAsync(request.Name, id, cancellationToken);

        category.Rename(name, slug);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var usage = (await repository.ListAsync(visibleAt: null, cancellationToken)).FirstOrDefault(item => item.Item.Id == id);

        return new TaxonomyItemResponse(category.Id, category.Name, category.Slug, usage?.PostCount ?? 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        repository.Remove(await GetOrThrowAsync(id, cancellationToken));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<(string Name, string Slug)> NameAndFreeSlugAsync(string rawName, Guid? excludingId, CancellationToken cancellationToken)
    {
        var name = TaxonomyText.CleanName(rawName);
        var slug = TaxonomyText.ToSlug(name);

        if (await repository.SlugExistsAsync(slug, excludingId, cancellationToken))
        {
            throw new ConflictException($"Já existe uma categoria com o nome “{name}” (ou um nome que vira o mesmo endereço).");
        }

        return (name, slug);
    }

    private async Task<Category> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Categoria", id);
}

internal sealed class TagService(ITagRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider) : ITagService
{
    public async Task<IReadOnlyList<PublicTaxonomyItemResponse>> ListPublicAsync(CancellationToken cancellationToken) =>
        [.. (await repository.ListAsync(timeProvider.GetUtcNow().UtcDateTime, cancellationToken))
            .Select(usage => new PublicTaxonomyItemResponse(usage.Item.Name, usage.Item.Slug, usage.PostCount))];

    public async Task<IReadOnlyList<TaxonomyItemResponse>> ListAsync(CancellationToken cancellationToken) =>
        [.. (await repository.ListAsync(visibleAt: null, cancellationToken))
            .Select(usage => new TaxonomyItemResponse(usage.Item.Id, usage.Item.Name, usage.Item.Slug, usage.PostCount))];

    public async Task<TaxonomyItemResponse> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken)
    {
        var tag = await repository.GetByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Tag", id);
        var name = TaxonomyText.CleanName(request.Name);
        var slug = TaxonomyText.ToSlug(name);

        if (await repository.SlugExistsAsync(slug, id, cancellationToken))
        {
            throw new ConflictException($"Já existe uma tag “{name}” (ou um nome que vira o mesmo endereço).");
        }

        tag.Rename(name, slug);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var usage = (await repository.ListAsync(visibleAt: null, cancellationToken)).FirstOrDefault(item => item.Item.Id == id);

        return new TaxonomyItemResponse(tag.Id, tag.Name, tag.Slug, usage?.PostCount ?? 0);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        repository.Remove(await repository.GetByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Tag", id));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
