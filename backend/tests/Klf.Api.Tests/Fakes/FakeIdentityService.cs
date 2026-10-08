using Klf.Application.Interfaces.Identity;

namespace Klf.Api.Tests.Fakes;

public sealed class FakeIdentityService(UserAccount user, CredentialsCheckResult? loginResult = null, string validPassword = KlfApiFactory.AdminPassword) : IIdentityService
{
    public const string ValidCode = "123456";
    public const string ValidRecoveryCode = "AAAAA-BBBBB";
    public const int MaxFailedAttempts = 5;

    private string _password = validPassword;

    public bool TwoFactorEnabled { get; set; } = user.TwoFactorEnabled;

    public HashSet<string> RecoveryCodes { get; } = [ValidRecoveryCode];

    public int FailedAttempts { get; private set; }

    public Task<CredentialsCheckResult> CheckCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (email != user.Email || password != _password)
        {
            return Task.FromResult(CredentialsCheckResult.Invalid);
        }

        var result = loginResult ?? CredentialsCheckResult.Success(user);

        return Task.FromResult(result.User is null ? result : CredentialsCheckResult.Success(Current));
    }

    public Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(userId == user.Id ? Current : null);

    public Task<TwoFactorSetup?> StartTwoFactorSetupAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult<TwoFactorSetup?>(userId == user.Id && !TwoFactorEnabled
            ? new TwoFactorSetup("abcd efgh", "otpauth://totp/KLF:admin?secret=ABCDEFGH&issuer=KLF")
            : null);

    public Task<TwoFactorCheckStatus> VerifyTwoFactorCodeAsync(Guid userId, string code, CancellationToken cancellationToken) =>
        Task.FromResult(Attempt(code == ValidCode));

    public Task<TwoFactorCheckStatus> RedeemRecoveryCodeAsync(Guid userId, string recoveryCode, CancellationToken cancellationToken) =>
        Task.FromResult(Attempt(RecoveryCodes.Remove(recoveryCode)));

    public Task<TwoFactorEnableResult> EnableTwoFactorAsync(Guid userId, string code, CancellationToken cancellationToken)
    {
        var status = Attempt(code == ValidCode);

        if (status == TwoFactorCheckStatus.Success)
        {
            TwoFactorEnabled = true;
        }

        return Task.FromResult(Result(status));
    }

    public Task<TwoFactorEnableResult> RegenerateRecoveryCodesAsync(Guid userId, string code, CancellationToken cancellationToken) =>
        Task.FromResult(Result(Attempt(code == ValidCode)));

    public const string ValidResetToken = "reset-token";

    public bool ResetTokenUsed { get; private set; }

    public string? PasswordAfterReset { get; private set; }

    public Task<PasswordResetToken?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult<PasswordResetToken?>(email == user.Email ? new PasswordResetToken(user.Email, user.FullName, ValidResetToken) : null);

    public Task<PasswordResetResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        if (email != user.Email || token != ValidResetToken || ResetTokenUsed)
        {
            return Task.FromResult(new PasswordResetResult(PasswordResetStatus.InvalidToken, [], null));
        }

        if (newPassword.Length < 10)
        {
            return Task.FromResult(new PasswordResetResult(PasswordResetStatus.WeakPassword, ["A senha deve ter pelo menos 10 caracteres."], null));
        }

        ResetTokenUsed = true;
        PasswordAfterReset = newPassword;
        FailedAttempts = 0;
        _password = newPassword;

        return Task.FromResult(new PasswordResetResult(PasswordResetStatus.Success, [], Current));
    }

    private UserAccount Current => user with { TwoFactorEnabled = TwoFactorEnabled };

    private TwoFactorEnableResult Result(TwoFactorCheckStatus status)
    {
        if (status != TwoFactorCheckStatus.Success)
        {
            return new TwoFactorEnableResult(status, []);
        }

        RecoveryCodes.Clear();
        string[] codes = [.. Enumerable.Range(1, 10).Select(i => $"CODE{i:00}-XXXXX")];
        RecoveryCodes.UnionWith(codes);

        return new TwoFactorEnableResult(status, codes);
    }

    private TwoFactorCheckStatus Attempt(bool succeeded)
    {
        if (FailedAttempts >= MaxFailedAttempts)
        {
            return TwoFactorCheckStatus.LockedOut;
        }

        if (succeeded)
        {
            FailedAttempts = 0;
            return TwoFactorCheckStatus.Success;
        }

        FailedAttempts++;

        return FailedAttempts >= MaxFailedAttempts ? TwoFactorCheckStatus.LockedOut : TwoFactorCheckStatus.Invalid;
    }
}
