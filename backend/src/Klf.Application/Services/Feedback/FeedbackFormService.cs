using Klf.Application.DTOs.Feedback;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Feedback;

internal sealed class FeedbackFormService(IFeedbackFormRepository repository, IUnitOfWork unitOfWork) : IFeedbackFormService
{
    public async Task<IReadOnlyList<FeedbackFormListItemResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var forms = await repository.ListAsync(cancellationToken);

        return [.. forms.Select(form => form.ToListItem())];
    }

    public async Task<FeedbackFormResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var form = await GetOrThrowAsync(id, cancellationToken);

        return form.ToResponse();
    }

    public async Task<FeedbackFormResponse> CreateAsync(CreateFeedbackFormRequest request, CancellationToken cancellationToken)
    {
        var form = new FeedbackForm(request.Title.Trim(), NullIfBlank(request.Description), request.Definition.ToDomain());

        repository.Add(form);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToResponse();
    }

    public async Task<FeedbackFormResponse> UpdateAsync(Guid id, UpdateFeedbackFormRequest request, CancellationToken cancellationToken)
    {
        var form = await GetOrThrowAsync(id, cancellationToken);

        form.Update(request.Title.Trim(), NullIfBlank(request.Description), request.Definition.ToDomain());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var form = await GetOrThrowAsync(id, cancellationToken);

        repository.Remove(form);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private async Task<FeedbackForm> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Formulário", id);
}
