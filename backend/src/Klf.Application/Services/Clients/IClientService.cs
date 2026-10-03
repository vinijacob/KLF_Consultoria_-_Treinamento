using Klf.Application.DTOs.Clients;

namespace Klf.Application.Services.Clients;

/// <summary>Manages the companies and stores trained by KLF, for the admin panel and the public site.</summary>
public interface IClientService
{
    /// <summary>Lists the active clients in display order (public site).</summary>
    Task<IReadOnlyList<PublicClientResponse>> ListPublicAsync(CancellationToken cancellationToken);

    /// <summary>Lists every client, active or not, in display order (admin panel).</summary>
    Task<IReadOnlyList<ClientResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns one client.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The client does not exist.</exception>
    Task<ClientResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds a client.</summary>
    Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces every editable field of a client.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The client does not exist.</exception>
    Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken);

    /// <summary>Soft-deletes a client; it disappears from the site but stays in the database.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The client does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
