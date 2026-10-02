using Klf.Application.DTOs.Catalog;

namespace Klf.Application.Services.Catalog;

/// <summary>Manages the catalog of services offered by KLF, for the admin panel and the public site.</summary>
public interface IServiceCatalogService
{
    /// <summary>Lists the active services in display order (public site).</summary>
    Task<IReadOnlyList<ServiceListItemResponse>> ListPublicAsync(CancellationToken cancellationToken);

    /// <summary>Returns an active service by its slug (public site).</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">No active service has this slug (inactive ones count as not found).</exception>
    Task<PublicServiceResponse> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Lists every service, active or not, in display order (admin panel).</summary>
    Task<IReadOnlyList<ServiceListItemResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns one service with every field.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The service does not exist.</exception>
    Task<ServiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a service.</summary>
    /// <exception cref="Domain.Exceptions.ConflictException">Another service already uses the slug.</exception>
    Task<ServiceResponse> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces every editable field of a service.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The service does not exist.</exception>
    /// <exception cref="Domain.Exceptions.ConflictException">Another service already uses the slug.</exception>
    Task<ServiceResponse> UpdateAsync(Guid id, UpdateServiceRequest request, CancellationToken cancellationToken);

    /// <summary>Soft-deletes a service; it disappears from the site but stays in the database.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The service does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
