using FluentValidation;

using Klf.Application.DTOs.DataRequests;
using Klf.Application.Validators.Common;
using Klf.Domain.Enums;

namespace Klf.Application.Validators.DataRequests;

/// <summary>Validates <see cref="CreateDataRequestRequest"/>.</summary>
public sealed class CreateDataRequestRequestValidator : AbstractValidator<CreateDataRequestRequest>
{
    /// <summary>Largest name, matching the database column.</summary>
    public const int NameMaxLength = 120;

    /// <summary>Largest e-mail, matching the database column.</summary>
    public const int EmailMaxLength = 200;

    /// <summary>Largest details text, matching the database column.</summary>
    public const int DetailsMaxLength = 2000;

    /// <summary>Defines the rules.</summary>
    public CreateDataRequestRequestValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe seu nome.")
            .MaximumLength(NameMaxLength).WithMessage($"O nome pode ter no máximo {NameMaxLength} caracteres.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe seu e-mail: é por ele que respondemos.")
            .MaximumLength(EmailMaxLength).WithMessage($"O e-mail pode ter no máximo {EmailMaxLength} caracteres.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Kind).IsInEnum().WithMessage("Escolha o que você quer pedir.");

        RuleFor(x => x.Details)
            .NotEmpty().When(x => x.Kind is DataRequestKind.Correction or DataRequestKind.Other)
            .WithMessage("Conte o que você precisa, para podermos atender.")
            .MaximumLength(DetailsMaxLength).WithMessage($"O texto pode ter no máximo {DetailsMaxLength} caracteres.");

        RuleFor(x => x.Website).MaximumLength(300);
    }
}

/// <summary>Validates <see cref="UpdateDataRequestStatusRequest"/>.</summary>
public sealed class UpdateDataRequestStatusRequestValidator : AbstractValidator<UpdateDataRequestStatusRequest>
{
    /// <summary>Defines the rules.</summary>
    public UpdateDataRequestStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Situação inválida. Use Open, Completed ou Declined.");

        RuleFor(x => x.Resolution)
            .MaximumLength(CreateDataRequestRequestValidator.DetailsMaxLength)
            .WithMessage($"O texto pode ter no máximo {CreateDataRequestRequestValidator.DetailsMaxLength} caracteres.");
    }
}

/// <summary>Validates <see cref="DataRequestListRequest"/>.</summary>
public sealed class DataRequestListRequestValidator : AbstractValidator<DataRequestListRequest>
{
    /// <summary>Defines the rules.</summary>
    public DataRequestListRequestValidator()
    {
        Include(new PagedRequestValidator());

        RuleFor(x => x.Status).IsInEnum().When(x => x.Status is not null).WithMessage("Situação inválida.");
    }
}

/// <summary>Validates <see cref="DataRequestFindingsRequest"/>.</summary>
public sealed class DataRequestFindingsRequestValidator : AbstractValidator<DataRequestFindingsRequest>
{
    /// <summary>Defines the rules.</summary>
    public DataRequestFindingsRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(CreateDataRequestRequestValidator.NameMaxLength).WithMessage("Nome comprido demais.");
    }
}
