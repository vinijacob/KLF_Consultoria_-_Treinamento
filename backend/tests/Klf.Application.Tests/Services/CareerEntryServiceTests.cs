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
        var request = new CreateCareerEntryRequest(CareerEntryType.Education, "  MBA  ", " FGV ", null, Start, null, null);

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
        var request = new UpdateCareerEntryRequest(CareerEntryType.Experience, "Gerente", "Loja X", "Desc", Start, Start.AddYears(1), null);

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

    [Fact]
    public async Task Create_puts_entry_at_the_end_of_its_group_unless_a_position_is_chosen()
    {
        var first = Seed(CareerEntryType.Education, 0);
        Seed(CareerEntryType.Education, 1);
        Seed(CareerEntryType.Experience, 0);
        var token = TestContext.Current.CancellationToken;

        var atEnd = await _service.CreateAsync(new CreateCareerEntryRequest(CareerEntryType.Education, "Pós", null, null, Start, null, null), token);
        var atTop = await _service.CreateAsync(new CreateCareerEntryRequest(CareerEntryType.Education, "Curso", null, null, Start, null, 0), token);

        Assert.Equal(2, atEnd.DisplayOrder);
        Assert.Equal(0, atTop.DisplayOrder);
        Assert.Equal(3, first.DisplayOrder);
    }

    [Fact]
    public async Task Update_swaps_positions_with_the_entry_that_held_the_chosen_one()
    {
        var first = Seed(CareerEntryType.Education, 0);
        Seed(CareerEntryType.Education, 1);
        var last = Seed(CareerEntryType.Education, 2);
        var request = new UpdateCareerEntryRequest(CareerEntryType.Education, "MBA", "FGV", null, Start, null, 0);

        await _service.UpdateAsync(last.Id, request, TestContext.Current.CancellationToken);

        Assert.Equal(0, last.DisplayOrder);
        Assert.Equal(2, first.DisplayOrder);
    }

    [Fact]
    public async Task Update_to_another_type_closes_the_gap_and_goes_to_the_end_of_the_new_group()
    {
        var moving = Seed(CareerEntryType.Education, 0);
        var stays = Seed(CareerEntryType.Education, 1);
        Seed(CareerEntryType.Certification, 0);
        var request = new UpdateCareerEntryRequest(CareerEntryType.Certification, "MBA", "FGV", null, Start, null, null);

        await _service.UpdateAsync(moving.Id, request, TestContext.Current.CancellationToken);

        Assert.Equal(1, moving.DisplayOrder);
        Assert.Equal(0, stays.DisplayOrder);
    }

    [Fact]
    public async Task Delete_closes_the_gap_in_the_group_only()
    {
        var deleted = Seed(CareerEntryType.Education, 0);
        var next = Seed(CareerEntryType.Education, 1);
        var other = Seed(CareerEntryType.Experience, 1);

        await _service.DeleteAsync(deleted.Id, TestContext.Current.CancellationToken);

        Assert.Equal(0, next.DisplayOrder);
        Assert.Equal(1, other.DisplayOrder);
    }

    [Fact]
    public async Task Position_outside_the_group_is_rejected_and_nothing_is_saved()
    {
        Seed(CareerEntryType.Education, 0);
        var request = new CreateCareerEntryRequest(CareerEntryType.Education, "Pós", null, null, Start, null, 5);

        var error = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request, TestContext.Current.CancellationToken));

        Assert.Equal(["Escolha uma posição de 0 a 1."], error.Errors["DisplayOrder"]);
        Assert.Single(_repository.Entries);
        Assert.Equal(0, _repository.SaveCount);
    }

    private CareerEntry Seed(CareerEntryType type = CareerEntryType.Education, int order = 0)
    {
        var entry = new CareerEntry(type, "MBA", "FGV", null, Start, null, order);
        _repository.Entries.Add(entry);
        return entry;
    }
}
