using Klf.Application.DTOs.Career;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Career;

internal sealed class CareerEntryService(ICareerEntryRepository repository, IUnitOfWork unitOfWork) : ICareerEntryService
{
    public async Task<IReadOnlyList<CareerEntryResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var entries = await repository.ListAsync(cancellationToken);

        return [.. entries.Select(entry => entry.ToResponse())];
    }

    public async Task<CareerEntryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entry = await GetOrThrowAsync(id, cancellationToken);

        return entry.ToResponse();
    }

    public async Task<CareerEntryResponse> CreateAsync(CreateCareerEntryRequest request, CancellationToken cancellationToken)
    {
        var group = await repository.ListInOrderAsync(request.EntryType, cancellationToken);
        var entry = new CareerEntry(
            request.EntryType,
            request.Title.Trim(),
            request.Institution?.Trim(),
            request.Description?.Trim(),
            request.StartDate,
            request.EndDate,
            group.Count);

        DisplayOrdering.Place(group, entry, request.DisplayOrder);
        repository.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entry.ToResponse();
    }

    public async Task<CareerEntryResponse> UpdateAsync(Guid id, UpdateCareerEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await GetOrThrowAsync(id, cancellationToken);
        var previousType = entry.EntryType;
        var group = await repository.ListInOrderAsync(request.EntryType, cancellationToken);
        var previousGroup = previousType == request.EntryType ? null : await repository.ListInOrderAsync(previousType, cancellationToken);

        entry.Update(
            request.EntryType,
            request.Title.Trim(),
            request.Institution?.Trim(),
            request.Description?.Trim(),
            request.StartDate,
            request.EndDate);

        if (previousGroup is not null)
        {
            DisplayOrdering.Remove(previousGroup, entry);
        }

        DisplayOrdering.Place(group, entry, request.DisplayOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entry.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entry = await GetOrThrowAsync(id, cancellationToken);
        var group = await repository.ListInOrderAsync(entry.EntryType, cancellationToken);

        DisplayOrdering.Remove(group, entry);
        repository.Remove(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CareerEntry> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Item da trajetória", id);
}
