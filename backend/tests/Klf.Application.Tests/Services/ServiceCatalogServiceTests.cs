using Klf.Application.DTOs.Catalog;
using Klf.Application.Services.Catalog;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class ServiceCatalogServiceTests
{
    private readonly InMemoryServiceRepository _repository = new();
    private readonly ServiceCatalogService _service;

    public ServiceCatalogServiceTests()
    {
        _service = new ServiceCatalogService(_repository, _repository, new FakeHtmlSanitizer());
    }

    [Fact]
    public async Task Create_saves_service_with_sanitized_html_and_trimmed_text()
    {
        var request = Request("lideranca") with { Title = "  Liderança  ", ContentHtml = "<p>ok</p><script>", Summary = "  " };

        var response = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Services);
        Assert.Equal("Liderança", saved.Title);
        Assert.Equal("<p>ok</p>", saved.ContentHtml);
        Assert.Null(saved.Summary);
        Assert.Equal(response.Id, saved.Id);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Create_throws_conflict_when_slug_is_already_used()
    {
        Seed("repetido");

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(Request("repetido"), TestContext.Current.CancellationToken));

        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Update_accepts_own_slug_but_rejects_slug_of_another_service()
    {
        var first = Seed("primeiro");
        Seed("segundo");
        var sameSlug = new UpdateServiceRequest("Novo", "primeiro", null, "<p>x</p>", "Todos", 8, ServiceFormat.Online, null, 0, true);

        await _service.UpdateAsync(first.Id, sameSlug, TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.UpdateAsync(first.Id, sameSlug with { Slug = "segundo" }, TestContext.Current.CancellationToken));

        Assert.Equal("Novo", first.Title);
    }

    [Fact]
    public async Task Update_throws_not_found_when_service_does_not_exist()
    {
        var request = new UpdateServiceRequest("T", "t", null, "<p>x</p>", "Todos", 8, ServiceFormat.Online, null, 0, true);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(Guid.CreateVersion7(), request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Delete_removes_service_when_it_exists()
    {
        var service = Seed("apagar");

        await _service.DeleteAsync(service.Id, TestContext.Current.CancellationToken);

        Assert.Empty(_repository.Services);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Public_list_hides_inactive_services_and_orders_by_display_order()
    {
        Seed("terceiro", order: 3);
        Seed("oculto", active: false);
        Seed("primeiro", order: 1);

        var list = await _service.ListPublicAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["primeiro", "terceiro"], list.Select(s => s.Slug));
    }

    [Fact]
    public async Task Admin_list_includes_inactive_services()
    {
        Seed("ativo");
        Seed("oculto", active: false);

        var list = await _service.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task Public_get_by_slug_throws_not_found_when_service_is_inactive()
    {
        Seed("oculto", active: false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetPublicBySlugAsync("oculto", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Public_get_by_slug_returns_detail_when_service_is_active()
    {
        Seed("aberto");

        var service = await _service.GetPublicBySlugAsync("aberto", TestContext.Current.CancellationToken);

        Assert.Equal("aberto", service.Slug);
        Assert.Equal("Todos", service.Audience);
    }

    private static CreateServiceRequest Request(string slug) =>
        new("Título", slug, null, "<p>x</p>", "Todos", 8, ServiceFormat.Online, null, 0, true);

    private Service Seed(string slug, int order = 0, bool active = true)
    {
        var service = new Service("Título " + slug, slug, null, "<p>x</p>", "Todos", 8, ServiceFormat.Online, null, order, active);
        _repository.Services.Add(service);
        return service;
    }
}
