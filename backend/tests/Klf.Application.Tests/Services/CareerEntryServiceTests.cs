using Klf.Application.DTOs.Career;
using Klf.Application.Services.Career;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class CareerEntryServiceTests
{
    private static readonly DateOnly Start = new(2020, 3, 1);

    private readonly InMemoryCareerEntryRepository _repository = new();
    private readonly CareerEntryService _service;

    public CareerEntryServiceTests()
    {
        _service = new CareerEntryService(_repository, _repository);
    }

    [Fact]
    public async Task Create_saves_trimmed_entry_when_request_is_valid()
    {
        var request = new CreateCareerEntryRequest(CareerEntryType.Education, "  MBA  ", " FGV ", null, Start, null, 1);

        var response = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

        var saved = Assert.Single(_repository.Entries);
        Assert.Equal("MBA", saved.Title);
        Assert.Equal("FGV", saved.Institution);
        Assert.Equal(saved.Id, response.Id);
        Assert.True(response.IsOngoing);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Update_changes_fields_when_entry_exists()
    {
        var entry = Seed();
        var request = new UpdateCareerEntryRequest(CareerEntryType.Experience, "Gerente", "Loja X", "Desc", Start, Start.AddYears(1), 2);

        var response = await _service.UpdateAsync(entry.Id, request, TestContext.Current.CancellationToken);

        Assert.Equal("Gerente", entry.Title);
        Assert.False(response.IsOngoing);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Update_throws_not_found_when_entry_does_not_exist()
    {
        var request = new UpdateCareerEntryRequest(CareerEntryType.Experience, "Gerente", null, null, Start, null, 0);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(Guid.CreateVersion7(), request, TestContext.Current.CancellationToken));

        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Delete_removes_entry_when_it_exists()
    {
        var entry = Seed();

        await _service.DeleteAsync(entry.Id, TestContext.Current.CancellationToken);

        Assert.Empty(_repository.Entries);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task List_returns_entries_ordered_by_type_then_display_order()
    {
        Seed(CareerEntryType.Experience, 0);
        Seed(CareerEntryType.Education, 2);
        Seed(CareerEntryType.Education, 1);

        var list = await _service.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [(CareerEntryType.Education, 1), (CareerEntryType.Education, 2), (CareerEntryType.Experience, 0)],
            list.Select(e => (e.EntryType, e.DisplayOrder)));
    }

    private CareerEntry Seed(CareerEntryType type = CareerEntryType.Education, int order = 0)
    {
        var entry = new CareerEntry(type, "MBA", "FGV", null, Start, null, order);
        _repository.Entries.Add(entry);
        return entry;
    }
}
