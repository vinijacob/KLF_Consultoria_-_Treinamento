using System.Text.Json;

using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Contact;
using Klf.Application.DTOs.Settings;
using Klf.Application.Interfaces.Email;
using Klf.Application.Interfaces.Links;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Contact;

internal sealed class ContactMessageService(
    IContactMessageRepository repository,
    IServiceRepository services,
    ISiteSettingRepository settings,
    IEmailSender emailSender,
    IFrontendLinks links,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IContactMessageService
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    public async Task SubmitAsync(CreateContactMessageRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return;
        }

        string? serviceTitle = null;

        if (request.ServiceId is { } serviceId)
        {
            var service = await services.GetByIdAsync(serviceId, cancellationToken);

            if (service is null || !service.IsActive)
            {
                throw new ValidationException("ServiceId", "Esse serviço não está mais disponível. Escolha outro ou deixe em branco.");
            }

            serviceTitle = service.Title;
        }

        var message = new ContactMessage(
            request.Name.Trim(),
            request.Email.Trim(),
            NullIfBlank(request.Phone),
            NullIfBlank(request.Company),
            request.ServiceId,
            serviceTitle,
            request.Message.Trim(),
            request.PrivacyPolicyVersion,
            timeProvider.GetUtcNow().UtcDateTime);

        repository.Add(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var notifyTo = await ContactEmailAsync(cancellationToken);

        if (notifyTo is not null)
        {
            await emailSender.SendAsync(ContactEmailTemplates.NewMessage(notifyTo, message, links.ContactMessage(message.Id)), cancellationToken);
        }
    }

    public async Task<PagedResponse<ContactMessageListItemResponse>> ListAsync(ContactMessageListRequest request, CancellationToken cancellationToken)
    {
        var (items, total) = await repository.ListAsync(request.Status, NullIfBlank(request.Search), request.Skip, request.PageSize, cancellationToken);

        return new PagedResponse<ContactMessageListItemResponse>([.. items.Select(item => item.ToListItem())], request.Page, request.PageSize, total);
    }

    public async Task<ContactMessageResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        (await GetOrThrowAsync(id, cancellationToken)).ToResponse();

    public async Task<ContactMessageResponse> UpdateStatusAsync(Guid id, UpdateContactMessageStatusRequest request, CancellationToken cancellationToken)
    {
        var message = await GetOrThrowAsync(id, cancellationToken);

        message.ChangeStatus(request.Status, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return message.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await GetOrThrowAsync(id, cancellationToken);

        repository.Remove(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ContactMessagesSummaryResponse> SummaryAsync(CancellationToken cancellationToken) =>
        new(await repository.CountAsync(ContactMessageStatus.New, cancellationToken));

    public Task<int> PurgeExpiredAsync(CancellationToken cancellationToken) =>
        repository.DeleteLastContactedBeforeAsync(ContactMessage.RetentionCutoff(timeProvider.GetUtcNow().UtcDateTime), cancellationToken);

    private async Task<string?> ContactEmailAsync(CancellationToken cancellationToken)
    {
        var setting = await settings.GetByKeyAsync(SiteSettingKeys.Contact, cancellationToken);

        if (setting is null)
        {
            return null;
        }

        var contact = JsonSerializer.Deserialize<ContactSettings>(setting.ValueJson, WebJson);

        return string.IsNullOrWhiteSpace(contact?.Email) ? null : contact.Email;
    }

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private async Task<ContactMessage> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Mensagem não encontrada. Ela pode ter sido excluída.");
}
