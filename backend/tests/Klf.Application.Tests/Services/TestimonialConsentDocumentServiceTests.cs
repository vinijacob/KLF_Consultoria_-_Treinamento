using System.Text;

using Klf.Application.DTOs.Testimonials;
using Klf.Application.Services.Testimonials;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class TestimonialConsentDocumentServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7\n1 0 obj\n<<>>\nendobj\n%%EOF");

    private readonly InMemoryTestimonialRepository _repository = new();
    private readonly FakePrivateFileStorage _storage = new();
    private readonly TestimonialConsentDocumentService _service;
    private readonly Testimonial _testimonial = new("Ana", null, null, "ok", null, Now.UtcDateTime.AddDays(-1), false, true, 0);

    public TestimonialConsentDocumentServiceTests()
    {
        _repository.Testimonials.Add(_testimonial);
        _service = new TestimonialConsentDocumentService(_repository, _repository, _storage, new MutableTimeProvider(Now));
    }

    [Fact]
    public async Task Attach_saves_the_pdf_in_private_storage_and_records_the_file_on_the_testimonial()
    {
        var response = await AttachAsync(Pdf, "C:\\Termos\\termo-ana.pdf");

        var (key, content) = Assert.Single(_storage.Files);
        Assert.StartsWith($"consent/{_testimonial.Id}/", key, StringComparison.Ordinal);
        Assert.Equal(Pdf, content);
        Assert.Equal("termo-ana.pdf", response.ConsentDocument!.FileName);
        Assert.Equal(Pdf.Length, response.ConsentDocument.SizeBytes);
        Assert.Equal(Now, response.ConsentDocument.UploadedAt);
        Assert.True(response.IsPublished);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Attach_replaces_the_previous_file_and_deletes_it()
    {
        await AttachAsync(Pdf, "antigo.pdf");
        var oldKey = _storage.Files.Keys.Single();

        await AttachAsync(Pdf, "novo.pdf");

        var newKey = Assert.Single(_storage.Files.Keys);
        Assert.NotEqual(oldKey, newKey);
        Assert.Equal("novo.pdf", _testimonial.ConsentDocumentName);
    }

    [Fact]
    public async Task Attach_rejects_a_file_that_is_not_a_pdf_even_with_pdf_extension()
    {
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        var error = await Assert.ThrowsAsync<ValidationException>(() => AttachAsync(png, "termo.pdf"));

        Assert.Contains("File", error.Errors.Keys);
        Assert.Empty(_storage.Files);
        Assert.Null(_testimonial.ConsentDocumentKey);
    }

    [Fact]
    public async Task Attach_rejects_a_file_larger_than_10_mb()
    {
        using var content = new MemoryStream(Pdf);
        var request = new UploadConsentDocumentRequest(content, "termo.pdf", ITestimonialConsentDocumentService.MaxFileBytes + 1);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.AttachAsync(_testimonial.Id, request, TestContext.Current.CancellationToken));

        Assert.Empty(_storage.Files);
    }

    [Fact]
    public async Task Download_returns_the_pdf_with_its_original_name()
    {
        await AttachAsync(Pdf, "termo-ana.pdf");

        var file = await _service.DownloadAsync(_testimonial.Id, TestContext.Current.CancellationToken);

        Assert.Equal(Pdf, file.Content);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("termo-ana.pdf", file.FileName);
    }

    [Fact]
    public async Task Download_throws_not_found_when_no_term_is_attached()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DownloadAsync(_testimonial.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Remove_deletes_the_file_and_clears_the_testimonial()
    {
        await AttachAsync(Pdf, "termo.pdf");

        var response = await _service.RemoveAsync(_testimonial.Id, TestContext.Current.CancellationToken);

        Assert.Null(response.ConsentDocument);
        Assert.Empty(_storage.Files);
        Assert.Null(_testimonial.ConsentDocumentKey);
    }

    [Fact]
    public async Task Attach_throws_not_found_when_the_testimonial_does_not_exist()
    {
        using var content = new MemoryStream(Pdf);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AttachAsync(
            Guid.CreateVersion7(),
            new UploadConsentDocumentRequest(content, "termo.pdf", Pdf.Length),
            TestContext.Current.CancellationToken));
    }

    private async Task<TestimonialResponse> AttachAsync(byte[] bytes, string fileName)
    {
        using var content = new MemoryStream(bytes);

        return await _service.AttachAsync(
            _testimonial.Id,
            new UploadConsentDocumentRequest(content, fileName, bytes.Length),
            TestContext.Current.CancellationToken);
    }
}
