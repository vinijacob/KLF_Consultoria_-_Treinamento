using Klf.Application.DTOs.Users;
using Klf.Application.Services.Users;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Common;
using Klf.Domain.Entities;
using Klf.Domain.Exceptions;

namespace Klf.Application.Tests.Services;

public sealed class UserManagementServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 13, 15, 0, 0, TimeSpan.Zero);

    private readonly InMemoryUserDirectory _directory = new();
    private readonly InMemoryRefreshTokenRepository _sessions = new();
    private readonly FakeEmailSender _email = new();
    private readonly MutableTimeProvider _clock = new(Now);
    private readonly FakeAuditTrail _audit = new();
    private readonly UserManagementService _service;
    private readonly Guid _kilciene;

    public UserManagementServiceTests()
    {
        _service = new UserManagementService(_directory, _sessions, _sessions, _email, new FakeFrontendLinks(), _clock, _audit);
        _kilciene = _directory.Seed("Kilciene", Roles.Admin).Id;
    }

    private RefreshToken OpenSession(Guid userId)
    {
        var token = new RefreshToken(userId, Guid.CreateVersion7(), Guid.NewGuid().ToString(), Now.UtcDateTime.AddHours(8));
        _sessions.Tokens.Add(token);
        return token;
    }

    [Fact]
    public async Task Invite_creates_a_pending_account_and_sends_a_link_valid_for_seven_days()
    {
        var invite = await _service.InviteAsync(_kilciene, new InviteUserRequest(" Ana Editora ", " ana@klf.test ", Roles.Editor), TestContext.Current.CancellationToken);

        Assert.Equal("Ana Editora", invite.User.FullName);
        Assert.Equal("ana@klf.test", invite.User.Email);
        Assert.True(invite.User.InvitePending);
        Assert.Equal(Now.UtcDateTime.AddDays(7), invite.ExpiresAt);
        Assert.Contains("aceitar-convite", invite.InviteLink.AbsoluteUri, StringComparison.Ordinal);
        var email = Assert.Single(_email.Sent);
        Assert.Equal("ana@klf.test", email.To);
        Assert.Contains("Kilciene convidou você", email.TextBody, StringComparison.Ordinal);
        Assert.Contains("Edição", email.TextBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invite_is_rejected_when_the_e_mail_is_already_in_use()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.InviteAsync(_kilciene, new InviteUserRequest("Outra", "KILCIENE@klf.test", Roles.Editor), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Nobody_can_deactivate_themselves_or_change_their_own_role()
    {
        var deactivate = new UpdateUserRequest("Kilciene", Roles.Admin, IsActive: false);
        var demote = new UpdateUserRequest("Kilciene", Roles.Editor, IsActive: true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(_kilciene, _kilciene, deactivate, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(_kilciene, _kilciene, demote, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task The_last_active_admin_cannot_be_demoted()
    {
        var outsider = Guid.CreateVersion7();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            _service.UpdateAsync(outsider, _kilciene, new UpdateUserRequest("Kilciene", Roles.Editor, true), TestContext.Current.CancellationToken));

        Assert.Contains("pelo menos uma pessoa ativa", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Deactivating_ends_the_open_sessions_but_renaming_does_not()
    {
        var editor = _directory.Seed("Bruno", Roles.Editor);
        var session = OpenSession(editor.Id);

        await _service.UpdateAsync(_kilciene, editor.Id, new UpdateUserRequest("Bruno Souza", Roles.Editor, true), TestContext.Current.CancellationToken);
        Assert.Null(session.RevokedAt);

        var updated = await _service.UpdateAsync(_kilciene, editor.Id, new UpdateUserRequest("Bruno Souza", Roles.Editor, false), TestContext.Current.CancellationToken);

        Assert.False(updated.IsActive);
        Assert.NotNull(session.RevokedAt);
        Assert.Equal(["user.updated", "user.deactivated"], _audit.Entries.Select(entry => entry.Action));
        Assert.All(_audit.Entries, entry => Assert.Equal(editor.Id, entry.TargetId));
    }

    [Fact]
    public async Task Changing_the_role_ends_the_open_sessions()
    {
        var editor = _directory.Seed("Bruno", Roles.Editor);
        var session = OpenSession(editor.Id);

        var updated = await _service.UpdateAsync(_kilciene, editor.Id, new UpdateUserRequest("Bruno", Roles.Admin, true), TestContext.Current.CancellationToken);

        Assert.Equal(Roles.Admin, updated.Role);
        Assert.NotNull(session.RevokedAt);
    }

    [Fact]
    public async Task Resetting_two_factor_works_only_for_someone_else_and_ends_their_sessions()
    {
        var editor = _directory.Seed("Bruno", Roles.Editor);
        var session = OpenSession(editor.Id);

        await Assert.ThrowsAsync<ConflictException>(() => _service.ResetTwoFactorAsync(_kilciene, _kilciene, TestContext.Current.CancellationToken));
        var reset = await _service.ResetTwoFactorAsync(_kilciene, editor.Id, TestContext.Current.CancellationToken);

        Assert.False(reset.TwoFactorEnabled);
        Assert.NotNull(session.RevokedAt);
        Assert.Contains(_email.Sent, message => message.To == editor.Email && message.Subject.Contains("duas etapas", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Invite_can_be_sent_again_only_while_the_password_was_not_chosen()
    {
        var pending = _directory.Seed("Ana", Roles.Editor, invitePending: true);
        var accepted = _directory.Seed("Bruno", Roles.Editor);

        var again = await _service.ResendInviteAsync(_kilciene, pending.Id, TestContext.Current.CancellationToken);

        Assert.Equal(pending.Id, again.User.Id);
        await Assert.ThrowsAsync<ConflictException>(() => _service.ResendInviteAsync(_kilciene, accepted.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Accepting_the_invite_reports_weak_password_and_invalid_link_by_field()
    {
        var pending = _directory.Seed("Ana", Roles.Editor, invitePending: true);

        var weak = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.AcceptInviteAsync(new AcceptInviteRequest(pending.Email, InMemoryUserDirectory.ValidInviteToken, "curta"), TestContext.Current.CancellationToken));
        var wrongToken = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.AcceptInviteAsync(new AcceptInviteRequest(pending.Email, "outro", "Senha@Forte123"), TestContext.Current.CancellationToken));
        await _service.AcceptInviteAsync(new AcceptInviteRequest(pending.Email, InMemoryUserDirectory.ValidInviteToken, "Senha@Forte123"), TestContext.Current.CancellationToken);

        Assert.Contains("NewPassword", weak.Errors.Keys);
        Assert.Contains("Token", wrongToken.Errors.Keys);
        Assert.False((await _service.GetAsync(pending.Id, TestContext.Current.CancellationToken)).InvitePending);
    }
}
