using FluentValidation;

using Klf.Application.DTOs.Auth;

namespace Klf.Application.Validators.Auth;

internal static class TwoFactorRules
{
    public static IRuleBuilderOptions<T, string> Token<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Informe o token da verificação. Entre novamente.");

    public static IRuleBuilderOptions<T, string> AuthenticatorCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Informe o código do aplicativo autenticador.")
            .Must(IsSixDigits).WithMessage("O código deve ter 6 dígitos.");

    private static bool IsSixDigits(string code)
    {
        var digits = code.Replace(" ", string.Empty, StringComparison.Ordinal);

        return digits.Length == 6 && digits.All(char.IsAsciiDigit);
    }
}

/// <summary>Validates <see cref="TwoFactorCodeRequest"/>.</summary>
public sealed class TwoFactorCodeRequestValidator : AbstractValidator<TwoFactorCodeRequest>
{
    /// <summary>Defines the rules.</summary>
    public TwoFactorCodeRequestValidator()
    {
        RuleFor(x => x.TwoFactorToken).Token();
        RuleFor(x => x.Code).Cascade(CascadeMode.Stop).AuthenticatorCode();
    }
}

/// <summary>Validates <see cref="TwoFactorRecoveryRequest"/>.</summary>
public sealed class TwoFactorRecoveryRequestValidator : AbstractValidator<TwoFactorRecoveryRequest>
{
    /// <summary>Largest recovery code length accepted (generous, only to reject abuse).</summary>
    public const int RecoveryCodeMaxLength = 30;

    /// <summary>Defines the rules.</summary>
    public TwoFactorRecoveryRequestValidator()
    {
        RuleFor(x => x.TwoFactorToken).Token();
        RuleFor(x => x.RecoveryCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o código de recuperação.")
            .MaximumLength(RecoveryCodeMaxLength).WithMessage("Código de recuperação inválido.");
    }
}

/// <summary>Validates <see cref="TwoFactorSetupRequest"/>.</summary>
public sealed class TwoFactorSetupRequestValidator : AbstractValidator<TwoFactorSetupRequest>
{
    /// <summary>Defines the rules.</summary>
    public TwoFactorSetupRequestValidator()
    {
        RuleFor(x => x.TwoFactorToken).Token();
    }
}

/// <summary>Validates <see cref="RegenerateRecoveryCodesRequest"/>.</summary>
public sealed class RegenerateRecoveryCodesRequestValidator : AbstractValidator<RegenerateRecoveryCodesRequest>
{
    /// <summary>Defines the rules.</summary>
    public RegenerateRecoveryCodesRequestValidator()
    {
        RuleFor(x => x.Code).Cascade(CascadeMode.Stop).AuthenticatorCode();
    }
}
