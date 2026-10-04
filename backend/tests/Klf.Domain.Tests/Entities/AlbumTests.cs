using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Domain.Tests.Entities;

public sealed class AlbumTests
{
    [Fact]
    public void Items_get_positions_in_the_given_order_when_set()
    {
        var a = Guid.CreateVersion7();
        var b = Guid.CreateVersion7();
        var album = new Album("Turma", "turma", null, null, 0, true);

        album.SetItems([(b, "segunda"), (a, null)]);

        var items = album.Items.OrderBy(i => i.DisplayOrder).ToList();
        Assert.Equal([b, a], items.Select(i => i.MediaAssetId));
        Assert.Equal("segunda", items[0].Caption);
    }

    [Fact]
    public void Existing_items_are_kept_and_missing_ones_removed_when_set_again()
    {
        var a = Guid.CreateVersion7();
        var b = Guid.CreateVersion7();
        var c = Guid.CreateVersion7();
        var album = new Album("Turma", "turma", null, null, 0, true);
        album.SetItems([(a, "velha"), (b, null)]);
        var keptA = album.Items.Single(i => i.MediaAssetId == a);

        album.SetItems([(c, null), (a, "nova")]);

        Assert.Equal(2, album.Items.Count);
        Assert.Same(keptA, album.Items.Single(i => i.MediaAssetId == a));
        Assert.Equal("nova", keptA.Caption);
        Assert.Equal(1, keptA.DisplayOrder);
        Assert.DoesNotContain(album.Items, i => i.MediaAssetId == b);
    }

    [Fact]
    public void Set_items_throws_when_same_image_appears_twice()
    {
        var a = Guid.CreateVersion7();
        var album = new Album("Turma", "turma", null, null, 0, true);

        var error = Assert.Throws<ValidationException>(() => album.SetItems([(a, null), (a, "dup")]));

        Assert.Contains("Items", error.Errors.Keys);
    }

    [Fact]
    public void Every_field_is_replaced_when_album_is_updated()
    {
        var cover = Guid.CreateVersion7();
        var album = new Album("A", "a", null, null, 0, true);

        album.Update("B", "b", "Desc", cover, 5, false);

        Assert.Equal("B", album.Title);
        Assert.Equal("b", album.Slug);
        Assert.Equal("Desc", album.Description);
        Assert.Equal(cover, album.CoverId);
        Assert.Equal(5, album.DisplayOrder);
        Assert.False(album.IsActive);
    }
}
