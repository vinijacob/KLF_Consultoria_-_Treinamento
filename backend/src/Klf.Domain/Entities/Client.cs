using Klf.Domain.Common;

namespace Klf.Domain.Entities;

/// <summary>
/// A company or store trained by KLF, shown as a logo on the public site.
/// </summary>
public sealed class Client : SoftDeletableEntity, IOrderedEntity
{
    /// <summary>Creates a client.</summary>
    /// <param name="name">Name of the company or store.</param>
    /// <param name="websiteUrl">Link to its site or social profile; optional.</param>
    /// <param name="logoId">Logo image id; optional.</param>
    /// <param name="displayOrder">Position in the public list; lower numbers come first.</param>
    /// <param name="isActive">Whether the client is visible on the public site.</param>
    public Client(string name, string? websiteUrl, Guid? logoId, int displayOrder, bool isActive)
    {
        Name = name;
        Update(name, websiteUrl, logoId, isActive);
        DisplayOrder = displayOrder;
    }

    /// <summary>Name of the company or store.</summary>
    public string Name { get; private set; }

    /// <summary>Link to its site or social profile; optional.</summary>
    public string? WebsiteUrl { get; private set; }

    /// <summary>Logo image (will reference a <c>MediaAsset</c> once the images module exists); optional.</summary>
    public Guid? LogoId { get; private set; }

    /// <summary>Position in the list; 0 comes first.</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>Whether the client is visible on the public site.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Replaces every editable field except the position, which changes through <see cref="DisplayOrdering"/>.</summary>
    /// <param name="name">Name of the company or store.</param>
    /// <param name="websiteUrl">Link to its site or social profile; optional.</param>
    /// <param name="logoId">Logo image id; optional.</param>
    /// <param name="isActive">Whether the client is visible on the public site.</param>
    public void Update(string name, string? websiteUrl, Guid? logoId, bool isActive)
    {
        Name = name;
        WebsiteUrl = websiteUrl;
        LogoId = logoId;
        IsActive = isActive;
    }

    /// <inheritdoc />
    public void MoveTo(int position) => DisplayOrder = position;
}
