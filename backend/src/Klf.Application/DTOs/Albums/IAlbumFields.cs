namespace Klf.Application.DTOs.Albums;

/// <summary>Editable fields shared by the create and update requests, so both use the same validation rules.</summary>
public interface IAlbumFields
{
    /// <summary>Name of the album.</summary>
    string Title { get; }

    /// <summary>URL-friendly identifier.</summary>
    string Slug { get; }

    /// <summary>Short text about the album.</summary>
    string? Description { get; }

    /// <summary>Cover image id.</summary>
    Guid? CoverId { get; }

    /// <summary>Position in the public list.</summary>
    int DisplayOrder { get; }

    /// <summary>Whether the album is visible on the public site.</summary>
    bool IsActive { get; }
}
