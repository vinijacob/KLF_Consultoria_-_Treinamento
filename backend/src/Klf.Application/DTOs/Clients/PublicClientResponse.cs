namespace Klf.Application.DTOs.Clients;

/// <summary>An active client as the public site shows it.</summary>
/// <param name="Id">Client identifier.</param>
/// <param name="Name">Name of the company or store.</param>
/// <param name="WebsiteUrl">Link to its site or social profile.</param>
/// <param name="LogoId">Logo image id.</param>
public sealed record PublicClientResponse(Guid Id, string Name, string? WebsiteUrl, Guid? LogoId);
