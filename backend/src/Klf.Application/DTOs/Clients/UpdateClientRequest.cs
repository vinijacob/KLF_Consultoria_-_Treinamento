namespace Klf.Application.DTOs.Clients;

/// <summary>Data to replace a client.</summary>
/// <param name="Name">Name of the company or store (up to 200 characters).</param>
/// <param name="WebsiteUrl">Link to its site or social profile; must start with <c>https://</c>; optional.</param>
/// <param name="LogoId">Logo image id; optional.</param>
/// <param name="DisplayOrder">Position in the list (0 = first); leave empty to keep the current one. If another item holds it, the two swap places.</param>
/// <param name="IsActive">Whether the client is visible on the public site.</param>
public sealed record UpdateClientRequest(
    string Name,
    string? WebsiteUrl,
    Guid? LogoId,
    int? DisplayOrder,
    bool IsActive) : IClientFields;
