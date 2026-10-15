using FluentValidation;

using Klf.Application.DTOs.Audit;
using Klf.Application.Validators.Common;
using Klf.Domain.Common;

namespace Klf.Application.Validators.Audit;

/// <summary>Validates <see cref="AuditLogListRequest"/>.</summary>
public sealed class AuditLogListRequestValidator : AbstractValidator<AuditLogListRequest>
{
    /// <summary>Defines the rules.</summary>
    public AuditLogListRequestValidator()
    {
        Include(new PagedRequestValidator());

        RuleFor(x => x.Area)
            .Must(area => area is null || AuditActions.Areas.ContainsKey(area))
            .WithMessage("Área inválida. Use content, contact, feedback, team ou access.");

        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From).When(x => x.From is not null && x.To is not null)
            .WithMessage("A data final precisa ser igual ou depois da inicial.");
    }
}
