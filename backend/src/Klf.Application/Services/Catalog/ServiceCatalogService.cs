using Klf.Application.DTOs.Catalog;
using Klf.Application.Interfaces.Content;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Catalog;

internal sealed class ServiceCatalogService(
    IServiceRepository repository,
    IUnitOfWork unitOfWork,
    IHtmlContentSanitizer sanitizer) : IServiceCatalogService
{
    public async Task<IReadOnlyList<ServiceListItemResponse>> ListPublicAsync(CancellationToken cancellationToken)
    {
        var services = await repository.ListAsync(onlyActive: true, cancellationToken);

        return [.. services.Select(service => service.ToListItem())];
    }

    public async Task<PublicServiceResponse> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var service = await repository.GetActiveBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException("Serviço não encontrado.");

        return service.ToPublicResponse();
    }

    public async Task<IReadOnlyList<ServiceListItemResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var services = await repository.ListAsync(onlyActive: false, cancellationToken);

        return [.. services.Select(service => service.ToListItem())];
    }

    public async Task<ServiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var service = await GetOrThrowAsync(id, cancellationToken);

        return service.ToResponse();
    }

    public async Task<ServiceResponse> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken)
    {
        await EnsureSlugIsFreeAsync(request.Slug, excludingId: null, cancellationToken);

        var service = new Service(
            request.Title.Trim(),
            request.Slug,
            NullIfBlank(request.Summary),
            sanitizer.Sanitize(request.ContentHtml),
            request.Audience.Trim(),
            request.WorkloadHours,
            request.Format,
            request.CoverId,
            request.DisplayOrder,
            request.IsActive);

        repository.Add(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return service.ToResponse();
    }

    public async Task<ServiceResponse> UpdateAsync(Guid id, UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await GetOrThrowAsync(id, cancellationToken);
        await EnsureSlugIsFreeAsync(request.Slug, excludingId: id, cancellationToken);

        service.Update(
            request.Title.Trim(),
            request.Slug,
            NullIfBlank(request.Summary),
            sanitizer.Sanitize(request.ContentHtml),
            request.Audience.Trim(),
            request.WorkloadHours,
            request.Format,
            request.CoverId,
            request.DisplayOrder,
            request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return service.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var service = await GetOrThrowAsync(id, cancellationToken);

        repository.Remove(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private async Task<Service> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Serviço", id);

    private async Task EnsureSlugIsFreeAsync(string slug, Guid? excludingId, CancellationToken cancellationToken)
    {
        if (await repository.SlugExistsAsync(slug, excludingId, cancellationToken))
        {
            throw new ConflictException($"Já existe um serviço com o endereço '{slug}'. Escolha outro slug.");
        }
    }
}
