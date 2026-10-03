using FluentValidation;

using Klf.Application.DTOs.Clients;

namespace Klf.Application.Validators.Clients;

/// <summary>Rules for every request that carries <see cref="IClientFields"/>.</summary>
/// <typeparam name="T">Request type.</typeparam>
public abstract class ClientFieldsValidator<T> : AbstractValidator<T>
    where T : IClientFields
{
    /// <summary>Largest name length, matching the database column.</summary>
    public const int NameMaxLength = 200;

    /// <summary>Largest link length, matching the database column.</summary>
    public const int WebsiteUrlMaxLength = 300;

    /// <summary>Defines the shared rules.</summary>
    protected ClientFieldsValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe o nome do cliente.")
            .MaximumLength(NameMaxLength).WithMessage($"O nome pode ter no máximo {NameMaxLength} caracteres.");

        RuleFor(x => x.WebsiteUrl)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(WebsiteUrlMaxLength).WithMessage($"O link pode ter no máximo {WebsiteUrlMaxLength} caracteres.")
            .Must(IsHttpsUrl).WithMessage("O link deve ser um endereço válido que comece com https://.")
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl));

        RuleFor(x => x.LogoId)
            .NotEqual(Guid.Empty).When(x => x.LogoId is not null)
            .WithMessage("Logo inválido.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("A ordem não pode ser negativa.");
    }

    private static bool IsHttpsUrl(string? url) =>
        Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
}

/// <summary>Validates <see cref="CreateClientRequest"/>.</summary>
public sealed class CreateClientRequestValidator : ClientFieldsValidator<CreateClientRequest>;

/// <summary>Validates <see cref="UpdateClientRequest"/>.</summary>
public sealed class UpdateClientRequestValidator : ClientFieldsValidator<UpdateClientRequest>;
