using Klf.Application.DTOs.Career;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
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
        var entry = new CareerEntry(
            request.EntryType,
            request.Title.Trim(),
            request.Institution?.Trim(),
            request.Description?.Trim(),
            request.StartDate,
            request.EndDate,
            request.DisplayOrder);

        repository.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entry.ToResponse();
    }

    public async Task<CareerEntryResponse> UpdateAsync(Guid id, UpdateCareerEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await GetOrThrowAsync(id, cancellationToken);

        entry.Update(
            request.EntryType,
            request.Title.Trim(),
            request.Institution?.Trim(),
            request.Description?.Trim(),
            request.StartDate,
            request.EndDate,
            request.DisplayOrder);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entry.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entry = await GetOrThrowAsync(id, cancellationToken);

        repository.Remove(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CareerEntry> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Item da trajetória", id);
}
