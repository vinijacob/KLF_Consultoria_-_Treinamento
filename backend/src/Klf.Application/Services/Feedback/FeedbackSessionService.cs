using Klf.Application.DTOs.Common;
using Klf.Application.DTOs.Feedback;
using Klf.Application.Interfaces.Documents;
using Klf.Application.Interfaces.Links;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Feedback;

internal sealed class FeedbackSessionService(
    IFeedbackSessionRepository sessions,
    IFeedbackFormRepository forms,
    IFeedbackResponseRepository responses,
    IClientRepository clients,
    IServiceRepository services,
    IUnitOfWork unitOfWork,
    IFrontendLinks links,
    IFeedbackPosterRenderer posterRenderer,
    TimeProvider timeProvider) : IFeedbackSessionService
{
    public async Task<PagedResponse<FeedbackSessionListItemResponse>> ListAsync(
        FeedbackSessionListRequest request,
        CancellationToken cancellationToken)
    {
        var filter = new FeedbackSessionFilter(request.ClientId, request.ServiceId, request.Search?.Trim());
        var (items, total) = await sessions.ListAsync(filter, request.Skip, request.PageSize, cancellationToken);
        var counts = await responses.CountBySessionsAsync([.. items.Select(session => session.Id)], cancellationToken);

        return new PagedResponse<FeedbackSessionListItemResponse>(
            [.. items.Select(session =>
            {
                var count = counts.GetValueOrDefault(session.Id);
                return session.ToListItem(session.GetStatus(UtcNow, count), count);
            })],
            request.Page,
            request.PageSize,
            total);
    }

    public async Task<FeedbackSessionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(id, cancellationToken);

        return await ToResponseAsync(session, cancellationToken);
    }

    public async Task<FeedbackSessionResponse> CreateAsync(Guid userId, CreateFeedbackSessionRequest request, CancellationToken cancellationToken)
    {
        var form = await forms.GetByIdAsync(request.FormId, cancellationToken)
            ?? throw new ValidationException("FormId", "Formulário não encontrado.");
        await EnsureLinksExistAsync(request, cancellationToken);

        var session = new FeedbackSession(
            userId,
            request.Title.Trim(),
            form.Id,
            form.Title,
            form.Description,
            form.Definition,
            request.OpensAt.UtcDateTime,
            request.ClosesAt.UtcDateTime,
            request.MaxResponses,
            request.ClientId,
            request.ServiceId);

        sessions.Add(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return session.ToResponse(session.GetStatus(UtcNow, 0), 0, links.FeedbackForm(session.PublicCode));
    }

    public async Task<FeedbackSessionResponse> UpdateAsync(Guid id, UpdateFeedbackSessionRequest request, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(id, cancellationToken);
        await EnsureLinksExistAsync(request, cancellationToken);

        session.Update(
            request.Title.Trim(),
            request.OpensAt.UtcDateTime,
            request.ClosesAt.UtcDateTime,
            request.MaxResponses,
            request.ClientId,
            request.ServiceId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(session, cancellationToken);
    }

    public async Task<FeedbackSessionResponse> ReplaceFormAsync(Guid id, ReplaceSessionFormRequest request, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(id, cancellationToken);
        var count = await responses.CountBySessionAsync(id, cancellationToken);

        session.ReplaceForm(
            session.FormId,
            request.FormTitle.Trim(),
            string.IsNullOrWhiteSpace(request.FormDescription) ? null : request.FormDescription.Trim(),
            request.Definition.ToDomain(),
            count);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return session.ToResponse(session.GetStatus(UtcNow, count), count, links.FeedbackForm(session.PublicCode));
    }

    public async Task<FeedbackSessionResponse> CloseAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(id, cancellationToken);

        session.Close(UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(session, cancellationToken);
    }

    public async Task<FeedbackSessionResponse> ReopenAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(id, cancellationToken);

        session.Reopen();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(session, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(id, cancellationToken);

        sessions.Remove(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<FeedbackSessionResultsResponse> GetResultsAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(id, cancellationToken);
        var answered = await responses.ListBySessionsAsync([id], cancellationToken);

        return FeedbackResults.ForSession(session, session.GetStatus(UtcNow, answered.Count), answered);
    }

    public async Task<FeedbackSummaryResponse> GetSummaryAsync(FeedbackSummaryRequest request, CancellationToken cancellationToken)
    {
        var filter = new FeedbackSessionFilter(
            request.ClientId,
            request.ServiceId,
            OpensFrom: request.From is { } from ? AppTimeZone.ToUtc(from.ToDateTime(TimeOnly.MinValue)) : null,
            OpensBefore: request.To is { } to ? AppTimeZone.ToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue)) : null);

        var found = await sessions.ListAllAsync(filter, cancellationToken);
        var answered = await responses.ListBySessionsAsync([.. found.Select(session => session.Id)], cancellationToken);
        var bySession = answered.ToLookup(response => response.SessionId);

        var rows = found.Select(session =>
        {
            var sessionResponses = bySession[session.Id].ToList();
            var nps = sessionResponses.Count >= FeedbackResults.MinimumResponses
                ? FeedbackResults.PrimaryNps(session, sessionResponses)
                : null;

            return (Session: session, Responses: sessionResponses, Nps: nps);
        }).ToList();

        var npsValues = rows
            .Where(row => row.Responses.Count >= FeedbackResults.MinimumResponses)
            .SelectMany(row => FeedbackResults.PrimaryNpsValues(row.Session, row.Responses))
            .ToList();

        return new FeedbackSummaryResponse(
            rows.Count,
            answered.Count,
            npsValues.Count >= FeedbackResults.MinimumResponses ? FeedbackResults.Nps(npsValues) : null,
            [.. rows.Select(row => new SessionSummary(
                row.Session.Id,
                row.Session.Title,
                FeedbackMappings.Utc(row.Session.OpensAt),
                row.Responses.Count,
                row.Nps))]);
    }

    public async Task<FileDownload> GetQrCodeAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(id, cancellationToken);
        var png = posterRenderer.RenderQrCodePng(links.FeedbackForm(session.PublicCode));

        return new FileDownload(png, "image/png", $"qrcode-{session.PublicCode}.png");
    }

    public async Task<FileDownload> GetPosterAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await GetSessionAsync(id, cancellationToken);
        var pdf = posterRenderer.RenderPosterPdf(new FeedbackPoster(
            session.Title,
            session.FormTitle,
            links.FeedbackForm(session.PublicCode),
            AppTimeZone.ToLocal(FeedbackMappings.Utc(session.OpensAt)),
            AppTimeZone.ToLocal(FeedbackMappings.Utc(session.ClosesAt))));

        return new FileDownload(pdf, "application/pdf", $"cartaz-avaliacao-{session.PublicCode}.pdf");
    }

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    private async Task<FeedbackSessionResponse> ToResponseAsync(FeedbackSession session, CancellationToken cancellationToken)
    {
        var count = await responses.CountBySessionAsync(session.Id, cancellationToken);

        return session.ToResponse(session.GetStatus(UtcNow, count), count, links.FeedbackForm(session.PublicCode));
    }

    private async Task<FeedbackSession> GetSessionAsync(Guid id, CancellationToken cancellationToken) =>
        await sessions.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Sessão de avaliação", id);

    private async Task EnsureLinksExistAsync(IFeedbackSessionFields request, CancellationToken cancellationToken)
    {
        if (request.ClientId is { } clientId && await clients.GetByIdAsync(clientId, cancellationToken) is null)
        {
            throw new ValidationException("ClientId", "Cliente não encontrado.");
        }

        if (request.ServiceId is { } serviceId && await services.GetByIdAsync(serviceId, cancellationToken) is null)
        {
            throw new ValidationException("ServiceId", "Serviço não encontrado.");
        }
    }
}
