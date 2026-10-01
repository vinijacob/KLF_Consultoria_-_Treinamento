using Klf.Application.Interfaces.Identity;

using Microsoft.AspNetCore.Identity;

namespace Klf.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<CredentialsCheckResult> CheckCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return CredentialsCheckResult.Invalid;
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return CredentialsCheckResult.LockedOut;
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);

            return await userManager.IsLockedOutAsync(user)
                ? CredentialsCheckResult.LockedOut
                : CredentialsCheckResult.Invalid;
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return CredentialsCheckResult.Success(await ToAccountAsync(user));
    }

    public async Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        return user is null ? null : await ToAccountAsync(user);
    }

    private async Task<UserAccount> ToAccountAsync(ApplicationUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.FullName, [.. await userManager.GetRolesAsync(user)]);
}
