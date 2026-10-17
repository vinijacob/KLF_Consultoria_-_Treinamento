using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Posts;
using Klf.Application.Interfaces.Content;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
using Klf.Application.Services.Media;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Posts;

internal sealed class PostService(
    IPostRepository repository,
    IMediaAssetRepository mediaRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork,
    IHtmlContentSanitizer sanitizer,
    TimeProvider timeProvider) : IPostService
{
    public async Task<PagedResponse<PostListItemResponse>> ListPublicAsync(PostListRequest request, CancellationToken cancellationToken)
    {
        var filter = new PostFilter(
            request.Type,
            Search: request.Search?.Trim(),
            VisibleAt: UtcNow,
            CategorySlug: NullIfBlank(request.Category),
            TagSlug: NullIfBlank(request.Tag));

        return await ListPageAsync(filter, request, cancellationToken);
    }

    public async Task<PublicPostResponse> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var post = await repository.GetVisibleBySlugAsync(slug, UtcNow, cancellationToken)
            ?? throw new NotFoundException("Post não encontrado.");

        return post.ToPublicResponse();
    }

    public async Task<PagedResponse<PostListItemResponse>> ListAsync(AdminPostListRequest request, CancellationToken cancellationToken)
    {
        var filter = new PostFilter(
            request.Type,
            request.Status,
            request.Search?.Trim(),
            CategorySlug: NullIfBlank(request.Category),
            TagSlug: NullIfBlank(request.Tag));

        return await ListPageAsync(filter, request, cancellationToken);
    }

    public async Task<PostResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var post = await GetOrThrowAsync(id, cancellationToken);

        return post.ToResponse();
    }

    public async Task<PostResponse> CreateAsync(Guid authorId, CreatePostRequest request, CancellationToken cancellationToken)
    {
        await EnsureSlugIsFreeAsync(request.Slug, excludingId: null, cancellationToken);
        await MediaReference.EnsureExistsAsync(mediaRepository, request.CoverId, "CoverId", cancellationToken);
        var category = await ResolveCategoryAsync(request.CategoryId, cancellationToken);
        var tags = await ResolveTagsAsync(request.Tags, cancellationToken);

        var post = new Post(
            authorId,
            request.Type,
            request.Title.Trim(),
            request.Slug,
            NullIfBlank(request.Summary),
            request.ContentJson,
            sanitizer.Sanitize(request.ContentHtml),
            request.CoverId,
            request.Status,
            request.ScheduledFor?.UtcDateTime,
            NullIfBlank(request.SeoTitle),
            NullIfBlank(request.SeoDescription),
            UtcNow);

        post.Classify(category, tags);
        repository.Add(post);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return post.ToResponse();
    }

    public async Task<PostResponse> UpdateAsync(Guid id, UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var post = await GetOrThrowAsync(id, cancellationToken);
        await EnsureSlugIsFreeAsync(request.Slug, excludingId: id, cancellationToken);
        await MediaReference.EnsureExistsAsync(mediaRepository, request.CoverId, "CoverId", cancellationToken);
        var category = await ResolveCategoryAsync(request.CategoryId, cancellationToken);
        var tags = await ResolveTagsAsync(request.Tags, cancellationToken);

        post.Update(
            request.Type,
            request.Title.Trim(),
            request.Slug,
            NullIfBlank(request.Summary),
            request.ContentJson,
            sanitizer.Sanitize(request.ContentHtml),
            request.CoverId,
            request.Status,
            request.ScheduledFor?.UtcDateTime,
            NullIfBlank(request.SeoTitle),
            NullIfBlank(request.SeoDescription),
            UtcNow);
        post.Classify(category, tags);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return post.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var post = await GetOrThrowAsync(id, cancellationToken);

        repository.Remove(post);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private async Task<PagedResponse<PostListItemResponse>> ListPageAsync(PostFilter filter, PagedRequest paging, CancellationToken cancellationToken)
    {
        var (posts, total) = await repository.ListAsync(filter, paging.Skip, paging.PageSize, cancellationToken);

        return new PagedResponse<PostListItemResponse>([.. posts.Select(post => post.ToListItem())], paging.Page, paging.PageSize, total);
    }

    private async Task<Category?> ResolveCategoryAsync(Guid? categoryId, CancellationToken cancellationToken) =>
        categoryId is { } id
            ? await categoryRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new ValidationException("CategoryId", "Essa categoria não existe mais. Escolha outra.")
            : null;

    /// <summary>Finds the tags by name (ignoring accents and case) and creates the missing ones, keeping the given order.</summary>
    private async Task<IReadOnlyList<Tag>> ResolveTagsAsync(IReadOnlyList<string>? names, CancellationToken cancellationToken)
    {
        var wanted = (names ?? [])
            .Select(TaxonomyText.CleanName)
            .Select(name => (Name: name, Slug: TaxonomyText.ToSlug(name)))
            .Where(tag => tag.Slug.Length > 0)
            .DistinctBy(tag => tag.Slug)
            .ToList();

        if (wanted.Count == 0)
        {
            return [];
        }

        var existing = (await tagRepository.FindBySlugsAsync([.. wanted.Select(tag => tag.Slug)], cancellationToken))
            .ToDictionary(tag => tag.Slug, StringComparer.Ordinal);

        return [.. wanted.Select(tag =>
        {
            if (existing.TryGetValue(tag.Slug, out var found))
            {
                return found;
            }

            var created = new Tag(tag.Name, tag.Slug);
            tagRepository.Add(created);

            return created;
        })];
    }

    private async Task<Post> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Post", id);

    private async Task EnsureSlugIsFreeAsync(string slug, Guid? excludingId, CancellationToken cancellationToken)
    {
        if (await repository.SlugExistsAsync(slug, excludingId, cancellationToken))
        {
            throw new ConflictException($"Já existe um post com o endereço '{slug}'. Escolha outro slug.");
        }
    }
}
