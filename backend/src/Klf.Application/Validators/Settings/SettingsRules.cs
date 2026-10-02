using FluentValidation;

namespace Klf.Application.Validators.Settings;

internal static class SettingsRules
{
    public static IRuleBuilderOptions<T, string?> MustBeHttpsUrl<T>(this IRuleBuilder<T, string?> rule, int maxLength = 300) => rule
        .Must(value => string.IsNullOrWhiteSpace(value) || IsHttpsUrl(value))
        .WithMessage("Informe um link válido que comece com https://.")
        .Must(value => value is null || value.Length <= maxLength)
        .WithMessage($"O link pode ter no máximo {maxLength} caracteres.");

    public static IRuleBuilderOptions<T, IReadOnlyList<string>?> MustBeShortList<T>(this IRuleBuilder<T, IReadOnlyList<string>?> rule, string itemName) => rule
        .Must(list => list is null || list.Count <= 20)
        .WithMessage($"Informe no máximo 20 {itemName}.")
        .Must(list => list is null || list.All(item => !string.IsNullOrWhiteSpace(item) && item.Length <= 200))
        .WithMessage($"Cada item de {itemName} deve ter entre 1 e 200 caracteres.");

    private static bool IsHttpsUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
}
