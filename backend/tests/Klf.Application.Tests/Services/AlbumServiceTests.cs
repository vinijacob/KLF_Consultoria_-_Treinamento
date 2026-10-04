using Klf.Application.DTOs.Albums;
using Klf.Application.Services.Albums;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class AlbumServiceTests
{
    private readonly InMemoryAlbumRepository _repository = new();
    private readonly InMemoryMediaAssetRepository _media = new();
    private readonly FakeFileStorage _storage = new();
    private readonly AlbumService _service;

    public AlbumServiceTests()
    {
        _service = new AlbumService(_repository, _media, _repository, _storage);
    }

    [Fact]
    public async Task Create_saves_empty_album_with_trimmed_text_and_cover_url()
    {
        var cover = _media.Seed();

        var response = await _service.CreateAsync(
            new CreateAlbumRequest("  Turma 1  ", "turma-1", " ", cover.Id, 0, true),
            TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Albums);
        Assert.Equal("Turma 1", saved.Title);
        Assert.Null(saved.Description);
        Assert.Equal(_storage.GetPublicUrl(cover.StorageKey), response.CoverUrl);
        Assert.Empty(response.Items);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Create_throws_conflict_for_used_slug_and_validation_for_missing_cover()
    {
        _repository.Albums.Add(new Album("A", "repetido", null, null, 0, true));
        var token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(new CreateAlbumRequest("B", "repetido", null, null, 0, true), token));
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(new CreateAlbumRequest("B", "novo", null, Guid.CreateVersion7(), 0, true), token));

        Assert.Contains("CoverId", error.Errors.Keys);
    }

    [Fact]
    public async Task Set_items_saves_images_in_order_with_urls_and_captions()
    {
        var first = _media.Seed("primeira");
        var second = _media.Seed("segunda");
        var album = new Album("A", "a", null, null, 0, true);
        _repository.Albums.Add(album);

        var response = await _service.SetItemsAsync(
            album.Id,
            new SetAlbumItemsRequest([new AlbumItemRequest(second.Id, " Legenda "), new AlbumItemRequest(first.Id, null)]),
            TestContext.Current.CancellationToken);

        Assert.Equal([second.Id, first.Id], response.Items.Select(i => i.MediaAssetId));
        Assert.Equal("Legenda", response.Items[0].Caption);
        Assert.Equal("segunda", response.Items[0].AltText);
        Assert.Equal(_storage.GetPublicUrl(second.StorageKey), response.Items[0].Url);
    }

    [Fact]
    public async Task Set_items_throws_validation_when_an_image_does_not_exist()
    {
        var album = new Album("A", "a", null, null, 0, true);
        _repository.Albums.Add(album);

        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SetItemsAsync(album.Id, new SetAlbumItemsRequest([new AlbumItemRequest(Guid.CreateVersion7(), null)]), TestContext.Current.CancellationToken));

        Assert.Contains("Items", error.Errors.Keys);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Public_list_returns_only_active_albums_with_item_count()
    {
        var image = _media.Seed();
        var active = new Album("Ativo", "ativo", null, null, 0, true);
        active.SetItems([(image.Id, null)]);
        _repository.Albums.Add(active);
        _repository.Albums.Add(new Album("Oculto", "oculto", null, null, 1, false));

        var list = await _service.ListPublicAsync(TestContext.Current.CancellationToken);

        var item = Assert.Single(list);
        Assert.Equal("ativo", item.Slug);
        Assert.Equal(1, item.ItemCount);
    }

    [Fact]
    public async Task Public_get_returns_404_for_inactive_album_and_hides_missing_images()
    {
        var image = _media.Seed();
        var album = new Album("Ativo", "ativo", null, null, 0, true);
        album.SetItems([(image.Id, "ok"), (Guid.CreateVersion7(), "sumiu")]);
        _repository.Albums.Add(album);
        _repository.Albums.Add(new Album("Oculto", "oculto", null, null, 1, false));
        var token = TestContext.Current.CancellationToken;

        var response = await _service.GetPublicBySlugAsync("ativo", token);

        Assert.Single(response.Items);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPublicBySlugAsync("oculto", token));
    }

    [Fact]
    public async Task Update_accepts_own_slug_and_delete_removes_album()
    {
        var album = new Album("A", "a", null, null, 0, true);
        _repository.Albums.Add(album);
        var token = TestContext.Current.CancellationToken;

        var updated = await _service.UpdateAsync(album.Id, new UpdateAlbumRequest("Novo", "a", null, null, 2, false), token);
        await _service.DeleteAsync(album.Id, token);

        Assert.Equal("Novo", updated.Title);
        Assert.Empty(_repository.Albums);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(album.Id, token));
    }
}
