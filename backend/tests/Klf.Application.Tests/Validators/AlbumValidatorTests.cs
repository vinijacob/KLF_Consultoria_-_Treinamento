using Klf.Application.DTOs.Albums;
using Klf.Application.DTOs.Media;
using Klf.Application.Validators.Albums;
using Klf.Application.Validators.Media;

namespace Klf.Application.Tests.Validators;

public sealed class AlbumValidatorTests
{
    [Fact]
    public void Album_is_valid_with_only_required_fields_and_rejects_bad_slug()
    {
        var validator = new CreateAlbumRequestValidator();

        Assert.True(validator.Validate(new CreateAlbumRequest("Turma", "turma", null, null, 0, true)).IsValid);
        Assert.Contains(
            validator.Validate(new CreateAlbumRequest("Turma", "Com Espaço", null, null, 0, true)).Errors,
            e => e.PropertyName == "Slug");
    }

    [Fact]
    public void Items_are_rejected_when_over_limit_or_caption_too_long_or_id_empty()
    {
        var validator = new SetAlbumItemsRequestValidator();
        var many = Enumerable.Range(0, 201).Select(_ => new AlbumItemRequest(Guid.CreateVersion7(), null)).ToList();

        Assert.False(validator.Validate(new SetAlbumItemsRequest(many)).IsValid);
        Assert.False(validator.Validate(new SetAlbumItemsRequest([new AlbumItemRequest(Guid.CreateVersion7(), new string('x', 301))])).IsValid);
        Assert.False(validator.Validate(new SetAlbumItemsRequest([new AlbumItemRequest(Guid.Empty, null)])).IsValid);
        Assert.True(validator.Validate(new SetAlbumItemsRequest([])).IsValid);
    }

    [Fact]
    public void Alt_text_is_rejected_when_longer_than_200_characters()
    {
        var validator = new UpdateMediaRequestValidator();

        Assert.False(validator.Validate(new UpdateMediaRequest(new string('x', 201))).IsValid);
        Assert.True(validator.Validate(new UpdateMediaRequest(null)).IsValid);
    }
}
