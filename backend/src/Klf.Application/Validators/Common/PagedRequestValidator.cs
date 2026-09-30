using FluentValidation;

using Klf.Application.DTOs.Common;

namespace Klf.Application.Validators.Common;

public sealed class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    public PagedRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagedRequest.MaxPageSize)
            .WithMessage($"O tamanho da página deve estar entre 1 e {PagedRequest.MaxPageSize}.");
    }
}
