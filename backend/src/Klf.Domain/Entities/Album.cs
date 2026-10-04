using Klf.Domain.Common;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Entities;

/// <summary>A photo album shown on the public gallery.</summary>
public sealed class Album : SoftDeletableEntity
{
    private readonly List<AlbumMedia> _items = [];

    /// <summary>Creates an album, empty.</summary>
    /// <param name="title">Name of the album.</param>
    /// <param name="slug">Unique URL identifier.</param>
    /// <param name="description">Short text about the album; optional.</param>
    /// <param name="coverId">Cover image; optional.</param>
    /// <param name="displayOrder">Position in the public list; lower numbers come first.</param>
    /// <param name="isActive">Whether the album is visible on the public site.</param>
    public Album(string title, string slug, string? description, Guid? coverId, int displayOrder, bool isActive)
    {
        Title = title;
        Slug = slug;
        Update(title, slug, description, coverId, displayOrder, isActive);
    }

    /// <summary>Name of the album.</summary>
    public string Title { get; private set; }

    /// <summary>Unique, URL-friendly identifier among non-deleted albums.</summary>
    public string Slug { get; private set; }

    /// <summary>Short text about the album; optional.</summary>
    public string? Description { get; private set; }

    /// <summary>Cover image; optional.</summary>
    public Guid? CoverId { get; private set; }

    /// <summary>Position in the public list; lower numbers come first.</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>Whether the album is visible on the public site.</summary>
    public bool IsActive { get; private set; }

    /// <summary>The images of the album. Use <see cref="SetItems"/> to change them.</summary>
    public IReadOnlyCollection<AlbumMedia> Items => _items;

    /// <summary>Replaces every editable field except the items.</summary>
    /// <param name="title">Name of the album.</param>
    /// <param name="slug">Unique URL identifier.</param>
    /// <param name="description">Short text about the album; optional.</param>
    /// <param name="coverId">Cover image; optional.</param>
    /// <param name="displayOrder">Position in the public list.</param>
    /// <param name="isActive">Whether the album is visible on the public site.</param>
    public void Update(string title, string slug, string? description, Guid? coverId, int displayOrder, bool isActive)
    {
        Title = title;
        Slug = slug;
        Description = description;
        CoverId = coverId;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    /// <summary>
    /// Replaces the images of the album, in the given order (the first one gets position 0).
    /// Images already in the album are kept (only caption and position change), so rows are not recreated.
    /// </summary>
    /// <param name="items">Images and captions in the desired order.</param>
    /// <exception cref="ValidationException">The same image appears twice.</exception>
    public void SetItems(IReadOnlyList<(Guid MediaAssetId, string? Caption)> items)
    {
        if (items.Select(item => item.MediaAssetId).Distinct().Count() != items.Count)
        {
            throw new ValidationException("Items", "A mesma imagem não pode aparecer duas vezes no álbum.");
        }

        _items.RemoveAll(existing => items.All(item => item.MediaAssetId != existing.MediaAssetId));

        for (var position = 0; position < items.Count; position++)
        {
            var (mediaAssetId, caption) = items[position];
            var existing = _items.Find(item => item.MediaAssetId == mediaAssetId);

            if (existing is null)
            {
                _items.Add(new AlbumMedia(mediaAssetId, caption, position));
            }
            else
            {
                existing.Update(caption, position);
            }
        }
    }
}
