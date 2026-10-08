using Klf.Application.DTOs.Feedback;
using Klf.Application.Interfaces.Repositories;
using Klf.Application.Mappings;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Enums;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Feedback;

internal sealed class PublicFeedbackService(
    IFeedbackSessionRepository sessions,
    IFeedbackResponseRepository responses,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IPublicFeedbackService
{
    public async Task<PublicFeedbackFormResponse> GetFormAsync(string publicCode, bool alreadyAnswered, CancellationToken cancellationToken)
    {
        var session = await GetOrThrowAsync(publicCode, cancellationToken);
        var count = await responses.CountBySessionAsync(session.Id, cancellationToken);
        var status = session.GetStatus(UtcNow, count);

        return new PublicFeedbackFormResponse(
            session.Title,
            session.FormTitle,
            session.FormDescription,
            status,
            FeedbackMappings.Utc(session.OpensAt),
            alreadyAnswered,
            status == FeedbackSessionStatus.Open && !alreadyAnswered ? session.Definition.ToDto() : null);
    }

    public async Task<FeedbackSubmission> SubmitAsync(
        string publicCode,
        SubmitFeedbackRequest request,
        bool alreadyAnswered,
        CancellationToken cancellationToken)
    {
        var session = await GetOrThrowAsync(publicCode, cancellationToken);

        if (alreadyAnswered)
        {
            throw new ConflictException("Você já respondeu esta avaliação. Obrigado!");
        }

        var count = await responses.CountBySessionAsync(session.Id, cancellationToken);

        switch (session.GetStatus(UtcNow, count))
        {
            case FeedbackSessionStatus.Scheduled:
                throw new ConflictException("Esta avaliação ainda não começou.");
            case FeedbackSessionStatus.Closed:
                throw new ConflictException("Esta avaliação já foi encerrada.");
        }

        var answers = session.Definition.NormalizeAnswers([.. request.Answers.Select(answer => answer.ToDomain())]);

        responses.Add(new FeedbackResponse(session.Id, AppTimeZone.Today(timeProvider), answers));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new FeedbackSubmission(session.Id, FeedbackMappings.Utc(session.ClosesAt).AddDays(1));
    }

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    private async Task<FeedbackSession> GetOrThrowAsync(string publicCode, CancellationToken cancellationToken) =>
        (publicCode.Length == FeedbackSession.PublicCodeLength
            ? await sessions.GetByPublicCodeAsync(publicCode, cancellationToken)
            : null)
        ?? throw new NotFoundException("Avaliação não encontrada. Confira o endereço ou o QR Code.");
}
