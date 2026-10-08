using Klf.Application.Interfaces.Identity;

using Microsoft.AspNetCore.Identity;

namespace Klf.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    private const string AuthenticatorIssuer = "KLF Consultoria";
    private const int RecoveryCodeCount = 10;

    private static readonly Dictionary<string, string> PasswordErrorMessages = new()
    {
        [nameof(IdentityErrorDescriber.PasswordTooShort)] = "A senha deve ter pelo menos 10 caracteres.",
        [nameof(IdentityErrorDescriber.PasswordRequiresDigit)] = "A senha deve ter pelo menos um número.",
        [nameof(IdentityErrorDescriber.PasswordRequiresLower)] = "A senha deve ter pelo menos uma letra minúscula.",
        [nameof(IdentityErrorDescriber.PasswordRequiresUpper)] = "A senha deve ter pelo menos uma letra maiúscula.",
        [nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric)] = "A senha deve ter pelo menos um símbolo (como ! @ # $).",
        [nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars)] = "A senha deve ter mais caracteres diferentes.",
    };

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

    public async Task<TwoFactorSetup?> StartTwoFactorSetupAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null || user.TwoFactorEnabled)
        {
            return null;
        }

        await userManager.ResetAuthenticatorKeyAsync(user);
        var key = await userManager.GetAuthenticatorKeyAsync(user) ?? throw new InvalidOperationException("Authenticator key was not created.");

        var issuer = Uri.EscapeDataString(AuthenticatorIssuer);
        var account = Uri.EscapeDataString(user.Email ?? user.UserName ?? userId.ToString());
        var uri = $"otpauth://totp/{issuer}:{account}?secret={key.ToUpperInvariant()}&issuer={issuer}&digits=6";

        return new TwoFactorSetup(FormatKey(key), uri);
    }

    public async Task<TwoFactorCheckStatus> VerifyTwoFactorCodeAsync(Guid userId, string code, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        return user is null ? TwoFactorCheckStatus.Invalid : await CheckAuthenticatorCodeAsync(user, code);
    }

    public async Task<TwoFactorCheckStatus> RedeemRecoveryCodeAsync(Guid userId, string recoveryCode, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return TwoFactorCheckStatus.Invalid;
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return TwoFactorCheckStatus.LockedOut;
        }

        var result = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, recoveryCode.Trim().ToUpperInvariant());

        return await RecordAttemptAsync(user, result.Succeeded);
    }

    public async Task<TwoFactorEnableResult> EnableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return new TwoFactorEnableResult(TwoFactorCheckStatus.Invalid, []);
        }

        var status = await CheckAuthenticatorCodeAsync(user, code);

        if (status != TwoFactorCheckStatus.Success)
        {
            return new TwoFactorEnableResult(status, []);
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);

        return new TwoFactorEnableResult(status, await NewRecoveryCodesAsync(user));
    }

    public async Task<TwoFactorEnableResult> RegenerateRecoveryCodesAsync(Guid userId, string code, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return new TwoFactorEnableResult(TwoFactorCheckStatus.Invalid, []);
        }

        var status = await CheckAuthenticatorCodeAsync(user, code);

        return status == TwoFactorCheckStatus.Success
            ? new TwoFactorEnableResult(status, await NewRecoveryCodesAsync(user))
            : new TwoFactorEnableResult(status, []);
    }

    public async Task<PasswordResetToken?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user?.Email is null)
        {
            return null;
        }

        return new PasswordResetToken(user.Email, user.FullName, await userManager.GeneratePasswordResetTokenAsync(user));
    }

    public async Task<PasswordResetResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return new PasswordResetResult(PasswordResetStatus.InvalidToken, [], null);
        }

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            await userManager.ResetAccessFailedCountAsync(user);
            await userManager.SetLockoutEndDateAsync(user, null);

            return new PasswordResetResult(PasswordResetStatus.Success, [], await ToAccountAsync(user));
        }

        if (result.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.InvalidToken)))
        {
            return new PasswordResetResult(PasswordResetStatus.InvalidToken, [], null);
        }

        var messages = result.Errors.Select(error => PasswordErrorMessages.GetValueOrDefault(error.Code, "A senha não atende aos requisitos.")).Distinct();

        return new PasswordResetResult(PasswordResetStatus.WeakPassword, [.. messages], null);
    }

    private async Task<TwoFactorCheckStatus> CheckAuthenticatorCodeAsync(ApplicationUser user, string code)
    {
        if (await userManager.IsLockedOutAsync(user))
        {
            return TwoFactorCheckStatus.LockedOut;
        }

        var normalized = code.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);
        var valid = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, normalized);

        return await RecordAttemptAsync(user, valid);
    }

    private async Task<TwoFactorCheckStatus> RecordAttemptAsync(ApplicationUser user, bool succeeded)
    {
        if (succeeded)
        {
            await userManager.ResetAccessFailedCountAsync(user);

            return TwoFactorCheckStatus.Success;
        }

        await userManager.AccessFailedAsync(user);

        return await userManager.IsLockedOutAsync(user) ? TwoFactorCheckStatus.LockedOut : TwoFactorCheckStatus.Invalid;
    }

    private async Task<IReadOnlyList<string>> NewRecoveryCodesAsync(ApplicationUser user) =>
        [.. await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RecoveryCodeCount) ?? []];

    private static string FormatKey(string key) =>
        string.Join(' ', Enumerable.Range(0, (key.Length + 3) / 4).Select(i => key.Substring(i * 4, Math.Min(4, key.Length - (i * 4))))).ToLowerInvariant();

    private async Task<UserAccount> ToAccountAsync(ApplicationUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.FullName, [.. await userManager.GetRolesAsync(user)], user.TwoFactorEnabled);
}
