using System.Security.Cryptography;
using System.Text.Json;

using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.DataRequests;
using Klf.Application.DTOs.Settings;
using Klf.Application.Interfaces.Audit;
using Klf.Application.Interfaces.Email;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Links;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Interfaces.Storage;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.DataRequests;

internal sealed class DataRequestService(
    IDataRequestRepository repository,
    IContactMessageRepository contactMessages,
    ITestimonialRepository testimonials,
    IMediaAssetRepository media,
    IFileStorage files,
    IPrivateFileStorage privateFiles,
    IUserDirectory users,
    ISiteSettingRepository settings,
    IEmailSender emailSender,
    IFrontendLinks links,
    IAuditTrail auditTrail,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IDataRequestService
{
    private const string ProtocolAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int ExcerptLength = 120;
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);
    private static readonly Func<Testimonial, bool> IsVisible = Testimonial.IsVisible.Compile();

    public static string KindLabel(DataRequestKind kind) => kind switch
    {
        DataRequestKind.Access => "Saber quais dados a KLF tem",
        DataRequestKind.Correction => "Corrigir dados",
        DataRequestKind.Deletion => "Apagar os dados",
        DataRequestKind.ConsentWithdrawal => "Retirar um consentimento",
        _ => "Outro pedido",
    };

    public async Task<DataRequestReceiptResponse> SubmitAsync(CreateDataRequestRequest request, CancellationToken cancellationToken)
    {
        var now = UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return new DataRequestReceiptResponse(NewProtocol(), DateOnly.FromDateTime(AppTimeZone.ToLocal(now)).AddDays(DataRequest.DeadlineDays));
        }

        var protocol = NewProtocol();

        while (await repository.ProtocolExistsAsync(protocol, cancellationToken))
        {
            protocol = NewProtocol();
        }

        var dataRequest = new DataRequest(
            protocol,
            request.Name.Trim(),
            request.Email.Trim(),
            request.Kind,
            string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            now);

        repository.Add(dataRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (await NotifyEmailAsync(cancellationToken) is { } notifyTo)
        {
            await emailSender.SendAsync(
                DataRequestEmailTemplates.NewRequest(notifyTo, dataRequest, KindLabel(dataRequest.Kind), links.DataRequest(dataRequest.Id)),
                cancellationToken);
        }

        return new DataRequestReceiptResponse(dataRequest.Protocol, dataRequest.DueOn);
    }

    public async Task<PagedResponse<DataRequestListItemResponse>> ListAsync(DataRequestListRequest request, CancellationToken cancellationToken)
    {
        var (items, total) = await repository.ListAsync(request.Status, request.Skip, request.PageSize, cancellationToken);
        var today = Today;

        return new PagedResponse<DataRequestListItemResponse>(
            [.. items.Select(item => new DataRequestListItemResponse(
                item.Id, item.Protocol, item.Name, item.Email, item.Kind, item.Status, item.ReceivedAt, item.DueOn, item.IsOverdue(today)))],
            request.Page,
            request.PageSize,
            total);
    }

    public async Task<DataRequestResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        ToResponse(await GetOrThrowAsync(id, cancellationToken));

    public async Task<DataRequestsSummaryResponse> SummaryAsync(CancellationToken cancellationToken)
    {
        var (open, overdue) = await repository.CountOpenAsync(Today, cancellationToken);

        return new DataRequestsSummaryResponse(open, overdue);
    }

    public async Task<DataRequestResponse> UpdateStatusAsync(Guid id, UpdateDataRequestStatusRequest request, CancellationToken cancellationToken)
    {
        var dataRequest = await GetOrThrowAsync(id, cancellationToken);

        dataRequest.ChangeStatus(request.Status, string.IsNullOrWhiteSpace(request.Resolution) ? null : request.Resolution.Trim(), UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(dataRequest);
    }

    public async Task<DataRequestFindingsResponse> FindDataAsync(Guid id, DataRequestFindingsRequest request, CancellationToken cancellationToken)
    {
        var dataRequest = await GetOrThrowAsync(id, cancellationToken);
        var name = string.IsNullOrWhiteSpace(request.Name) ? dataRequest.Name : request.Name.Trim();

        var messages = await contactMessages.ListByEmailAsync(dataRequest.Email, cancellationToken);
        var found = await testimonials.SearchByAuthorAsync(name, cancellationToken);

        return new DataRequestFindingsResponse(
            name,
            [.. messages.Select(message => new FoundContactMessage(message.Id, message.ReceivedAt, Excerpt(message.Text)))],
            [.. found.Select(testimonial => new FoundTestimonial(
                testimonial.Id,
                testimonial.AuthorName,
                testimonial.CompanyName,
                IsVisible(testimonial) && testimonial.DeletedAt is null,
                testimonial.DeletedAt is not null,
                testimonial.PhotoId is not null,
                testimonial.ConsentDocumentKey is not null))],
            await users.EmailExistsAsync(dataRequest.Email, cancellationToken));
    }

    public async Task<DataRequestResponse> EraseContactMessagesAsync(Guid id, CancellationToken cancellationToken)
    {
        var dataRequest = await GetOrThrowAsync(id, cancellationToken);
        var deleted = await contactMessages.DeleteByEmailAsync(dataRequest.Email, cancellationToken);
        var label = deleted == 1 ? "1 mensagem de contato" : $"{deleted} mensagens de contato";

        dataRequest.Record($"Apagadas de vez: {label} do e-mail do pedido.", UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditTrail.RecordAsync(new AuditEntry(AuditActions.DataRequestContactErased, dataRequest.Id, label), cancellationToken);

        return ToResponse(dataRequest);
    }

    public async Task<DataRequestResponse> EraseTestimonialAsync(Guid id, Guid testimonialId, CancellationToken cancellationToken)
    {
        var dataRequest = await GetOrThrowAsync(id, cancellationToken);
        var testimonial = await testimonials.GetIncludingDeletedAsync(testimonialId, cancellationToken)
            ?? throw NotFoundException.For("Depoimento", testimonialId);

        await testimonials.EraseAsync(testimonial.Id, cancellationToken);

        var erased = new List<string> { "depoimento" };

        if (testimonial.ConsentDocumentKey is { } documentKey)
        {
            await privateFiles.DeleteAsync(documentKey, CancellationToken.None);
            erased.Add("termo assinado");
        }

        if (testimonial.PhotoId is { } photoId
            && !await media.IsReferencedAsync(photoId, cancellationToken)
            && await media.GetIncludingDeletedAsync(photoId, cancellationToken) is { } photo)
        {
            await media.EraseAsync(photo.Id, cancellationToken);
            await files.DeleteAsync(photo.StorageKey, CancellationToken.None);
            erased.Add("foto");
        }

        dataRequest.Record($"Apagados de vez: {string.Join(", ", erased)} de “{testimonial.AuthorName}”.", UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditTrail.RecordAsync(new AuditEntry(AuditActions.DataRequestTestimonialErased, dataRequest.Id), cancellationToken);

        return ToResponse(dataRequest);
    }

    public Task<int> PurgeExpiredAsync(CancellationToken cancellationToken) =>
        repository.DeleteClosedBeforeAsync(DataRequest.RetentionCutoff(UtcNow), cancellationToken);

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    private DateOnly Today => AppTimeZone.Today(timeProvider);

    private DataRequestResponse ToResponse(DataRequest request) => new(
        request.Id,
        request.Protocol,
        request.Name,
        request.Email,
        request.Kind,
        request.Details,
        request.Status,
        request.ReceivedAt,
        request.DueOn,
        request.IsOverdue(Today),
        request.ClosedAt,
        request.Resolution,
        [.. request.History.Select(item => new DataRequestEventResponse(item.At, item.Text))],
        request.ClosedAt?.AddYears(DataRequest.RetentionYears));

    private static string NewProtocol()
    {
        var chars = new char[8];

        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = ProtocolAlphabet[RandomNumberGenerator.GetInt32(ProtocolAlphabet.Length)];
        }

        return $"{new string(chars, 0, 4)}-{new string(chars, 4, 4)}";
    }

    private static string Excerpt(string text)
    {
        var line = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return line.Length > ExcerptLength ? $"{line[..ExcerptLength]}…" : line;
    }

    private async Task<string?> NotifyEmailAsync(CancellationToken cancellationToken)
    {
        if (await settings.GetByKeyAsync(SiteSettingKeys.Legal, cancellationToken) is { } legalSetting
            && JsonSerializer.Deserialize<LegalSettings>(legalSetting.ValueJson, WebJson)?.DpoEmail is { Length: > 0 } dpoEmail)
        {
            return dpoEmail;
        }

        if (await settings.GetByKeyAsync(SiteSettingKeys.Contact, cancellationToken) is { } contactSetting
            && JsonSerializer.Deserialize<ContactSettings>(contactSetting.ValueJson, WebJson)?.Email is { Length: > 0 } contactEmail)
        {
            return contactEmail;
        }

        return null;
    }

    private async Task<DataRequest> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Pedido não encontrado. Ele pode ter sido excluído depois do prazo de guarda.");
}
