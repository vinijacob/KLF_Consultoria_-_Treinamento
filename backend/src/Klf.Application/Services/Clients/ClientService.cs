using Klf.Application.DTOs.Clients;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Clients;

internal sealed class ClientService(IClientRepository repository, IUnitOfWork unitOfWork) : IClientService
{
    public async Task<IReadOnlyList<PublicClientResponse>> ListPublicAsync(CancellationToken cancellationToken)
    {
        var clients = await repository.ListAsync(onlyActive: true, cancellationToken);

        return [.. clients.Select(client => client.ToPublicResponse())];
    }

    public async Task<IReadOnlyList<ClientResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var clients = await repository.ListAsync(onlyActive: false, cancellationToken);

        return [.. clients.Select(client => client.ToResponse())];
    }

    public async Task<ClientResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var client = await GetOrThrowAsync(id, cancellationToken);

        return client.ToResponse();
    }

    public async Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken)
    {
        var client = new Client(
            request.Name.Trim(),
            NullIfBlank(request.WebsiteUrl),
            request.LogoId,
            request.DisplayOrder,
            request.IsActive);

        repository.Add(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return client.ToResponse();
    }

    public async Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken)
    {
        var client = await GetOrThrowAsync(id, cancellationToken);

        client.Update(
            request.Name.Trim(),
            NullIfBlank(request.WebsiteUrl),
            request.LogoId,
            request.DisplayOrder,
            request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return client.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var client = await GetOrThrowAsync(id, cancellationToken);

        repository.Remove(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private async Task<Client> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Cliente", id);
}
