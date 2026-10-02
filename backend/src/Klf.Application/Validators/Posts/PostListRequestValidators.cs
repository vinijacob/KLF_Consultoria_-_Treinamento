using FluentValidation;

using Klf.Application.DTOs.Posts;
using Klf.Application.Validators.Common;

namespace Klf.Application.Validators.Posts;

/// <summary>Validates <see cref="PostListRequest"/>: pagination plus a bounded search text.</summary>
public sealed class PostListRequestValidator : AbstractValidator<PostListRequest>
{
    /// <summary>Largest search text.</summary>
    public const int SearchMaxLength = 100;

    /// <summary>Defines the rules.</summary>
    public PostListRequestValidator()
    {
        Include(new PagedRequestValidator());

        RuleFor(x => x.Type)
            .IsInEnum().When(x => x.Type is not null)
            .WithMessage("Tipo de post inválido. Use Project, Article ou News.");

        RuleFor(x => x.Search)
            .MaximumLength(SearchMaxLength).WithMessage($"A busca pode ter no máximo {SearchMaxLength} caracteres.");
    }
}

/// <summary>Validates <see cref="AdminPostListRequest"/>: everything in <see cref="PostListRequest"/> plus the status filter.</summary>
public sealed class AdminPostListRequestValidator : AbstractValidator<AdminPostListRequest>
{
    /// <summary>Defines the rules.</summary>
    public AdminPostListRequestValidator()
    {
        Include(new PostListRequestValidator());

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status is not null)
            .WithMessage("Status inválido. Use Draft, Scheduled ou Published.");
    }
}
