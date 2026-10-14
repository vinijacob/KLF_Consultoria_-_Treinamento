using Klf.Application.Interfaces.Identity;
using Klf.Domain.Common;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Klf.Infrastructure.Identity;

internal sealed class UserDirectory(UserManager<ApplicationUser> userManager) : IUserDirectory
{
    public async Task<IReadOnlyList<ManagedUser>> ListAsync(CancellationToken cancellationToken)
    {
        var users = await userManager.Users.AsNoTracking().OrderBy(user => user.FullName).ToListAsync(cancellationToken);
        var result = new List<ManagedUser>(users.Count);

        foreach (var user in users)
        {
            result.Add(await ToManagedAsync(user));
        }

        return result;
    }

    public async Task<ManagedUser?> FindAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        return user is null ? null : await ToManagedAsync(user);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        await userManager.FindByEmailAsync(email) is not null;

    public async Task<ManagedUser> CreateAsync(string email, string fullName, string role, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName };

        EnsureSucceeded(await userManager.CreateAsync(user));
        EnsureSucceeded(await userManager.AddToRoleAsync(user, role));

        return await ToManagedAsync(user);
    }

    public async Task<ManagedUser> UpdateAsync(Guid userId, string fullName, string role, bool isActive, CancellationToken cancellationToken)
    {
        var user = await GetAsync(userId);
        var deactivating = user.IsActive && !isActive;

        user.FullName = fullName;
        user.IsActive = isActive;
        EnsureSucceeded(await userManager.UpdateAsync(user));

        var current = await userManager.GetRolesAsync(user);

        if (!current.SequenceEqual([role]))
        {
            EnsureSucceeded(await userManager.RemoveFromRolesAsync(user, current));
            EnsureSucceeded(await userManager.AddToRoleAsync(user, role));
        }

        if (deactivating)
        {
            EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user));
        }

        return await ToManagedAsync(user);
    }

    public async Task<string> CreateInviteTokenAsync(Guid userId, CancellationToken cancellationToken) =>
        await userManager.GenerateUserTokenAsync(await GetAsync(userId), InviteTokenProvider.ProviderName, InviteTokenProvider.Purpose);

    public async Task<PasswordResetResult> AcceptInviteAsync(string email, string token, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive || user.PasswordHash is not null
            || !await userManager.VerifyUserTokenAsync(user, InviteTokenProvider.ProviderName, InviteTokenProvider.Purpose, token))
        {
            return new PasswordResetResult(PasswordResetStatus.InvalidToken, [], null);
        }

        var result = await userManager.AddPasswordAsync(user, password);

        if (!result.Succeeded)
        {
            return new PasswordResetResult(PasswordResetStatus.WeakPassword, [.. IdentityService.PasswordErrors(result)], null);
        }

        user.EmailConfirmed = true;
        EnsureSucceeded(await userManager.UpdateAsync(user));

        return new PasswordResetResult(PasswordResetStatus.Success, [], null);
    }

    public async Task ResetTwoFactorAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await GetAsync(userId);

        EnsureSucceeded(await userManager.SetTwoFactorEnabledAsync(user, false));
        EnsureSucceeded(await userManager.ResetAuthenticatorKeyAsync(user));
        await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 0);
    }

    private async Task<ApplicationUser> GetAsync(Guid userId) =>
        await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"User {userId} not found.");

    private async Task<ManagedUser> ToManagedAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault(Roles.All.Contains) ?? string.Empty;

        return new ManagedUser(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            role,
            user.IsActive,
            user.TwoFactorEnabled,
            user.PasswordHash is null,
            user.CreatedAt);
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}")));
        }
    }
}
