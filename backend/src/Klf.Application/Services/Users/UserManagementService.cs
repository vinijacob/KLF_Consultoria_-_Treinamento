using Klf.Application.DTOs.Users;
using Klf.Application.Interfaces.Audit;
using Klf.Application.Interfaces.Email;
using Klf.Application.Interfaces.Identity;
using Klf.Application.Interfaces.Links;
using Klf.Application.Interfaces.Repositories;
using Klf.Domain.Common;
using Klf.Domain.Exceptions;

namespace Klf.Application.Services.Users;

internal sealed class UserManagementService(
    IUserDirectory directory,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    IEmailSender emailSender,
    IFrontendLinks links,
    TimeProvider timeProvider,
    IAuditTrail auditTrail) : IUserManagementService
{
    private static readonly Dictionary<string, string> RoleLabels = new()
    {
        [Roles.Admin] = "Administração",
        [Roles.Editor] = "Edição",
    };

    public async Task<IReadOnlyList<UserResponse>> ListAsync(CancellationToken cancellationToken) =>
        [.. (await directory.ListAsync(cancellationToken)).Select(ToResponse)];

    public async Task<UserResponse> GetAsync(Guid userId, CancellationToken cancellationToken) =>
        ToResponse(await GetOrThrowAsync(userId, cancellationToken));

    public async Task<UserInviteResponse> InviteAsync(Guid actorId, InviteUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        if (await directory.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Já existe uma conta com este e-mail.");
        }

        var user = await directory.CreateAsync(email, request.FullName.Trim(), request.Role, cancellationToken);
        var invite = await SendInviteAsync(actorId, user, cancellationToken);
        await AuditAsync(AuditActions.UserInvited, user, cancellationToken);

        return invite;
    }

    public async Task<UserInviteResponse> ResendInviteAsync(Guid actorId, Guid userId, CancellationToken cancellationToken)
    {
        var user = await GetOrThrowAsync(userId, cancellationToken);

        if (!user.InvitePending)
        {
            throw new ConflictException("Esta pessoa já aceitou o convite e escolheu a senha. Se ela esqueceu, use “Esqueci minha senha” no login.");
        }

        if (!user.IsActive)
        {
            throw new ConflictException("A conta está desativada. Reative antes de enviar o convite de novo.");
        }

        var invite = await SendInviteAsync(actorId, user, cancellationToken);
        await AuditAsync(AuditActions.UserInviteResent, user, cancellationToken);

        return invite;
    }

    public async Task<UserResponse> UpdateAsync(Guid actorId, Guid userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await GetOrThrowAsync(userId, cancellationToken);
        var losesAdmin = user.Role == Roles.Admin && user.IsActive && (request.Role != Roles.Admin || !request.IsActive);

        if (userId == actorId && !request.IsActive)
        {
            throw new ConflictException("Você não pode desativar a própria conta.");
        }

        if (userId == actorId && request.Role != user.Role)
        {
            throw new ConflictException("Você não pode mudar o próprio perfil. Peça para outra pessoa da administração.");
        }

        if (losesAdmin && await CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            throw new ConflictException("O painel precisa de pelo menos uma pessoa ativa na administração.");
        }

        var updated = await directory.UpdateAsync(userId, request.FullName.Trim(), request.Role, request.IsActive, cancellationToken);

        if ((!request.IsActive && user.IsActive) || request.Role != user.Role)
        {
            await EndSessionsAsync(userId, cancellationToken);
        }

        foreach (var action in Changes(user, updated))
        {
            await AuditAsync(action, updated, cancellationToken);
        }

        return ToResponse(updated);
    }

    public async Task<UserResponse> ResetTwoFactorAsync(Guid actorId, Guid userId, CancellationToken cancellationToken)
    {
        if (userId == actorId)
        {
            throw new ConflictException("Para mexer na sua própria verificação em duas etapas, use a página Segurança.");
        }

        var user = await GetOrThrowAsync(userId, cancellationToken);

        await directory.ResetTwoFactorAsync(userId, cancellationToken);
        await EndSessionsAsync(userId, cancellationToken);
        await AuditAsync(AuditActions.UserTwoFactorReset, user, cancellationToken);
        await emailSender.SendAsync(UserEmailTemplates.TwoFactorReset(user.Email, user.FullName), cancellationToken);

        return ToResponse(await GetOrThrowAsync(userId, cancellationToken));
    }

    public async Task AcceptInviteAsync(AcceptInviteRequest request, CancellationToken cancellationToken)
    {
        var result = await directory.AcceptInviteAsync(request.Email.Trim(), request.Token, request.NewPassword, cancellationToken);

        switch (result.Status)
        {
            case PasswordResetStatus.Success:
                await auditTrail.RecordAsync(new AuditEntry(AuditActions.UserInviteAccepted, ActorName: request.Email.Trim()), cancellationToken);
                return;
            case PasswordResetStatus.WeakPassword:
                throw new ValidationException(new Dictionary<string, string[]> { ["NewPassword"] = [.. result.Errors] });
            default:
                throw new ValidationException("Token", "Convite inválido, expirado ou já usado. Peça um novo convite à administração.");
        }
    }

    private async Task<UserInviteResponse> SendInviteAsync(Guid actorId, ManagedUser user, CancellationToken cancellationToken)
    {
        var token = await directory.CreateInviteTokenAsync(user.Id, cancellationToken);
        var link = links.Invite(user.Email, token);
        var inviter = (await directory.FindAsync(actorId, cancellationToken))?.FullName ?? "A administração";

        await emailSender.SendAsync(
            UserEmailTemplates.Invite(user.Email, user.FullName, inviter, RoleLabels.GetValueOrDefault(user.Role, user.Role), link, IUserDirectory.InviteLifetimeDays),
            cancellationToken);

        return new UserInviteResponse(ToResponse(user), link, timeProvider.GetUtcNow().UtcDateTime.AddDays(IUserDirectory.InviteLifetimeDays));
    }

    private static IEnumerable<string> Changes(ManagedUser before, ManagedUser after)
    {
        if (before.IsActive && !after.IsActive)
        {
            yield return AuditActions.UserDeactivated;
        }

        if (!before.IsActive && after.IsActive)
        {
            yield return AuditActions.UserReactivated;
        }

        if (before.Role != after.Role)
        {
            yield return AuditActions.UserRoleChanged;
        }

        if (before.FullName != after.FullName)
        {
            yield return AuditActions.UserUpdated;
        }
    }

    private Task AuditAsync(string action, ManagedUser user, CancellationToken cancellationToken) =>
        auditTrail.RecordAsync(new AuditEntry(action, user.Id, user.FullName), cancellationToken);

    private async Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken) =>
        (await directory.ListAsync(cancellationToken)).Count(user => user.IsActive && user.Role == Roles.Admin);

    private async Task EndSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var token in await refreshTokens.ListActiveByUserAsync(userId, now, cancellationToken))
        {
            token.Revoke(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<ManagedUser> GetOrThrowAsync(Guid userId, CancellationToken cancellationToken) =>
        await directory.FindAsync(userId, cancellationToken) ?? throw new NotFoundException("Pessoa não encontrada.");

    private static UserResponse ToResponse(ManagedUser user) => new(
        user.Id,
        user.Email,
        user.FullName,
        user.Role,
        user.IsActive,
        user.TwoFactorEnabled,
        user.InvitePending,
        user.CreatedAt);
}
