using Klf.Application.DTOs.Clients;
using Klf.Application.Services.Clients;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class ClientServiceTests
{
    private readonly InMemoryClientRepository _repository = new();
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _service = new ClientService(_repository, _repository);
    }

    [Fact]
    public async Task Create_saves_client_with_trimmed_text_and_blank_link_as_null()
    {
        await _service.CreateAsync(new CreateClientRequest("  Loja X  ", "  ", null, 0, true), TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Clients);
        Assert.Equal("Loja X", saved.Name);
        Assert.Null(saved.WebsiteUrl);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Public_list_returns_only_active_clients_in_order()
    {
        _repository.Clients.Add(new Client("B", null, null, 2, true));
        _repository.Clients.Add(new Client("Oculta", null, null, 0, false));
        _repository.Clients.Add(new Client("A", null, null, 1, true));

        var list = await _service.ListPublicAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["A", "B"], list.Select(c => c.Name));
    }

    [Fact]
    public async Task Update_replaces_fields_and_throws_not_found_when_client_does_not_exist()
    {
        var client = new Client("Antiga", null, null, 0, true);
        _repository.Clients.Add(client);

        var response = await _service.UpdateAsync(client.Id, new UpdateClientRequest("Nova", null, null, 3, false), TestContext.Current.CancellationToken);

        Assert.Equal("Nova", response.Name);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(Guid.CreateVersion7(), new UpdateClientRequest("X", null, null, 0, true), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Delete_removes_client_and_throws_not_found_when_client_does_not_exist()
    {
        var client = new Client("Loja", null, null, 0, true);
        _repository.Clients.Add(client);

        await _service.DeleteAsync(client.Id, TestContext.Current.CancellationToken);

        Assert.Empty(_repository.Clients);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(client.Id, TestContext.Current.CancellationToken));
    }
}
