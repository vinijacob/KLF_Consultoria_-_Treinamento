namespace Klf.Application.DTOs.Clients;

/// <summary>A client with every field, for the admin panel.</summary>
/// <param name="Id">Client identifier.</param>
/// <param name="Name">Name of the company or store.</param>
/// <param name="WebsiteUrl">Link to its site or social profile.</param>
/// <param name="LogoId">Logo image id.</param>
/// <param name="DisplayOrder">Position in the public list.</param>
/// <param name="IsActive">Whether the client is visible on the public site.</param>
public sealed record ClientResponse(
    Guid Id,
    string Name,
    string? WebsiteUrl,
    Guid? LogoId,
    int DisplayOrder,
    bool IsActive);
