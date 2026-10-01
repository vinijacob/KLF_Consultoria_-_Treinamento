using FluentValidation;

using Klf.Application.DTOs.Common;

namespace Klf.Application.Validators.Common;

/// <summary>Ensures the page number and page size are within the allowed range.</summary>
public sealed class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    /// <summary>Defines the pagination rules.</summary>
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
