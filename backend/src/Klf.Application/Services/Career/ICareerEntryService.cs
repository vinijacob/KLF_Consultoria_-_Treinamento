using Klf.Application.DTOs.Career;

namespace Klf.Application.Services.Career;

/// <summary>Manages Kilciene's career timeline.</summary>
public interface ICareerEntryService
{
    /// <summary>Returns every entry ordered by type and display order.</summary>
    Task<IReadOnlyList<CareerEntryResponse>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns one entry.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The entry does not exist.</exception>
    Task<CareerEntryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Adds an entry.</summary>
    Task<CareerEntryResponse> CreateAsync(CreateCareerEntryRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces every field of an entry.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The entry does not exist.</exception>
    Task<CareerEntryResponse> UpdateAsync(Guid id, UpdateCareerEntryRequest request, CancellationToken cancellationToken);

    /// <summary>Soft-deletes an entry; it disappears from the site but stays in the database.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">The entry does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
