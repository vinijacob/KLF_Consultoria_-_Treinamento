using Klf.Application.Interfaces.Identity;

namespace Klf.Application.Tests.Fakes;

internal sealed class InMemoryUserDirectory : IUserDirectory
{
    public const string ValidInviteToken = "invite-token";

    public List<ManagedUser> Users { get; } = [];

    public HashSet<Guid> TwoFactorResets { get; } = [];

    public Dictionary<Guid, string> Passwords { get; } = [];

    public ManagedUser Seed(string fullName, string role, bool isActive = true, bool invitePending = false, bool twoFactor = true)
    {
        var user = new ManagedUser(Guid.CreateVersion7(), $"{fullName.ToLowerInvariant().Replace(' ', '.')}@klf.test", fullName, role, isActive, twoFactor, invitePending, DateTime.UtcNow);
        Users.Add(user);
        return user;
    }

    public Task<IReadOnlyList<ManagedUser>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ManagedUser>>([.. Users.OrderBy(x => x.FullName)]);

    public Task<ManagedUser?> FindAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(x => x.Id == userId));

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(Users.Any(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<ManagedUser> CreateAsync(string email, string fullName, string role, CancellationToken cancellationToken)
    {
        var user = new ManagedUser(Guid.CreateVersion7(), email, fullName, role, true, false, true, DateTime.UtcNow);
        Users.Add(user);
        return Task.FromResult(user);
    }

    public Task<ManagedUser> UpdateAsync(Guid userId, string fullName, string role, bool isActive, CancellationToken cancellationToken)
    {
        var index = Users.FindIndex(x => x.Id == userId);
        Users[index] = Users[index] with { FullName = fullName, Role = role, IsActive = isActive };
        return Task.FromResult(Users[index]);
    }

    public Task<string> CreateInviteTokenAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(ValidInviteToken);

    public Task<PasswordResetResult> AcceptInviteAsync(string email, string token, string password, CancellationToken cancellationToken)
    {
        var index = Users.FindIndex(x => x.Email == email);

        if (index < 0 || token != ValidInviteToken || !Users[index].InvitePending || !Users[index].IsActive)
        {
            return Task.FromResult(new PasswordResetResult(PasswordResetStatus.InvalidToken, [], null));
        }

        if (password.Length < 10)
        {
            return Task.FromResult(new PasswordResetResult(PasswordResetStatus.WeakPassword, ["A senha deve ter pelo menos 10 caracteres."], null));
        }

        Users[index] = Users[index] with { InvitePending = false };
        Passwords[Users[index].Id] = password;
        return Task.FromResult(new PasswordResetResult(PasswordResetStatus.Success, [], null));
    }

    public Task ResetTwoFactorAsync(Guid userId, CancellationToken cancellationToken)
    {
        var index = Users.FindIndex(x => x.Id == userId);
        Users[index] = Users[index] with { TwoFactorEnabled = false };
        TwoFactorResets.Add(userId);
        return Task.CompletedTask;
    }
}
