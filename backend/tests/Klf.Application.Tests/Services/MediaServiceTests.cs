using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Media;
using Klf.Application.Services.Media;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class MediaServiceTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0];
    private static readonly byte[] Webp = [.. "RIFF"u8, 0, 0, 0, 0, .. "WEBP"u8, 0];

    private readonly InMemoryMediaAssetRepository _repository = new();
    private readonly FakeFileStorage _storage = new();
    private readonly MediaService _service;

    public MediaServiceTests()
    {
        _service = new MediaService(_repository, _repository, _storage, new MutableTimeProvider(new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero)));
    }

    [Theory]
    [InlineData("png", "image/png", ".png")]
    [InlineData("jpeg", "image/jpeg", ".jpg")]
    [InlineData("webp", "image/webp", ".webp")]
    public async Task Upload_stores_file_and_metadata_with_detected_type_when_image_is_valid(string kind, string contentType, string extension)
    {
        var bytes = kind switch { "png" => Png, "jpeg" => Jpeg, _ => Webp };

        var response = await _service.UploadAsync(Request(bytes, "foto.exe", "  Equipe  "), TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Media);
        Assert.Equal(contentType, saved.ContentType);
        Assert.StartsWith("media/2026/10/", saved.StorageKey, StringComparison.Ordinal);
        Assert.EndsWith(extension, saved.StorageKey, StringComparison.Ordinal);
        Assert.Equal("Equipe", saved.AltText);
        Assert.Equal(bytes.Length, saved.SizeBytes);
        Assert.Equal(bytes, _storage.Files[saved.StorageKey].Content);
        Assert.Equal(_storage.GetPublicUrl(saved.StorageKey), response.Url);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Upload_ignores_user_path_in_file_name_and_never_uses_it_in_the_key()
    {
        await _service.UploadAsync(Request(Png, "../../etc/passwd.png"), TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Media);
        Assert.Equal("passwd.png", saved.OriginalFileName);
        Assert.DoesNotContain("passwd", saved.StorageKey, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Upload_rejects_file_when_content_is_not_a_supported_image_even_with_image_name()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UploadAsync(Request("<svg onload=alert(1)>"u8.ToArray(), "logo.png"), TestContext.Current.CancellationToken));

        Assert.Contains("File", error.Errors.Keys);
        Assert.Empty(_storage.Files);
        Assert.Empty(_repository.Media);
    }

    [Fact]
    public async Task Upload_rejects_empty_and_oversized_files()
    {
        var token = TestContext.Current.CancellationToken;
        var tooBig = new byte[IMediaService.MaxFileBytes + 1];

        await Assert.ThrowsAsync<ValidationException>(() => _service.UploadAsync(Request([], "a.png"), token));
        await Assert.ThrowsAsync<ValidationException>(() => _service.UploadAsync(Request(tooBig, "a.png"), token));
        Assert.Empty(_storage.Files);
    }

    [Fact]
    public async Task Upload_rejects_alt_text_longer_than_200_characters()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UploadAsync(Request(Png, "a.png", new string('x', 201)), TestContext.Current.CancellationToken));

        Assert.Contains("AltText", error.Errors.Keys);
    }

    [Fact]
    public async Task Update_changes_alt_text_and_blank_clears_it()
    {
        var media = _repository.Seed("antigo");

        var response = await _service.UpdateAsync(media.Id, new UpdateMediaRequest("  "), TestContext.Current.CancellationToken);

        Assert.Null(response.AltText);
    }

    [Fact]
    public async Task Delete_removes_row_and_file_when_image_is_not_in_use()
    {
        var media = _repository.Seed();
        await _storage.SaveAsync(media.StorageKey, new MemoryStream(Png), "image/png", TestContext.Current.CancellationToken);

        await _service.DeleteAsync(media.Id, TestContext.Current.CancellationToken);

        Assert.Empty(_repository.Media);
        Assert.Empty(_storage.Files);
    }

    [Fact]
    public async Task Delete_throws_conflict_and_keeps_file_when_image_is_in_use()
    {
        var media = _repository.Seed();
        _repository.Referenced.Add(media.Id);
        await _storage.SaveAsync(media.StorageKey, new MemoryStream(Png), "image/png", TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(media.Id, TestContext.Current.CancellationToken));

        Assert.Single(_repository.Media);
        Assert.Single(_storage.Files);
    }

    [Fact]
    public async Task Get_public_url_and_get_throw_not_found_when_image_does_not_exist()
    {
        var token = TestContext.Current.CancellationToken;

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPublicUrlAsync(Guid.CreateVersion7(), token));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.CreateVersion7(), token));
    }

    [Fact]
    public async Task List_returns_requested_page_with_total()
    {
        _repository.Seed();
        _repository.Seed();
        _repository.Seed();

        var page = await _service.ListAsync(new PagedRequest { Page = 2, PageSize = 2 }, TestContext.Current.CancellationToken);

        Assert.Single(page.Items);
        Assert.Equal(3, page.TotalItems);
    }

    private static UploadMediaRequest Request(byte[] bytes, string fileName, string? altText = null) =>
        new(new MemoryStream(bytes), fileName, bytes.Length, altText);
}
