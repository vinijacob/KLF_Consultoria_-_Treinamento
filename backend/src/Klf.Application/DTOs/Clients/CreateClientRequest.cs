namespace Klf.Application.DTOs.Clients;

/// <summary>Data to add a client (company or store trained by KLF).</summary>
/// <param name="Name">Name of the company or store (up to 200 characters).</param>
/// <param name="WebsiteUrl">Link to its site or social profile; must start with <c>https://</c>; optional.</param>
/// <param name="LogoId">Logo image id; optional.</param>
/// <param name="DisplayOrder">Position in the public list; lower numbers come first.</param>
/// <param name="IsActive">Whether the client is visible on the public site.</param>
public sealed record CreateClientRequest(
    string Name,
    string? WebsiteUrl,
    Guid? LogoId,
    int DisplayOrder,
    bool IsActive) : IClientFields;
