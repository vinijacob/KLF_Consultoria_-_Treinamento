namespace Klf.Application.DTOs.Clients;

/// <summary>Editable fields shared by the create and update requests, so both use the same validation rules.</summary>
public interface IClientFields
{
    /// <summary>Name of the company or store.</summary>
    string Name { get; }

    /// <summary>Link to its site or social profile.</summary>
    string? WebsiteUrl { get; }

    /// <summary>Logo image id.</summary>
    Guid? LogoId { get; }

    /// <summary>Position in the public list.</summary>
    int DisplayOrder { get; }

    /// <summary>Whether the client is visible on the public site.</summary>
    bool IsActive { get; }
}
