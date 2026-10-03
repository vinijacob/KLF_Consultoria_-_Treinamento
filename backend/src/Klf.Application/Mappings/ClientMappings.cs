using Klf.Application.DTOs.Clients;
using Klf.Domain.Entities;

namespace Klf.Application.Mappings;

internal static class ClientMappings
{
    public static ClientResponse ToResponse(this Client client) => new(
        client.Id,
        client.Name,
        client.WebsiteUrl,
        client.LogoId,
        client.DisplayOrder,
        client.IsActive);

    public static PublicClientResponse ToPublicResponse(this Client client) => new(
        client.Id,
        client.Name,
        client.WebsiteUrl,
        client.LogoId);
}
