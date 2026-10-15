using FluentValidation;

using Klf.Application.DTOs.Testimonials;

namespace Klf.Application.Validators.Testimonials;

/// <summary>Validates <see cref="PublicTestimonialListRequest"/>: a bounded number of latest testimonials.</summary>
public sealed class PublicTestimonialListRequestValidator : AbstractValidator<PublicTestimonialListRequest>
{
    /// <summary>Largest value accepted for <see cref="PublicTestimonialListRequest.Latest"/>.</summary>
    public const int MaxLatest = 20;

    /// <summary>Defines the rules.</summary>
    public PublicTestimonialListRequestValidator()
    {
        RuleFor(x => x.Latest)
            .InclusiveBetween(1, MaxLatest).When(x => x.Latest is not null)
            .WithMessage($"Peça entre 1 e {MaxLatest} depoimentos.");
    }
}
