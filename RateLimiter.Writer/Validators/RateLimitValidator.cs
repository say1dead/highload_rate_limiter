using FluentValidation;
using RateLimiter.Writer.Domain;

namespace RateLimiter.Writer.Validators;

public class RateLimitValidator : AbstractValidator<RateLimit>
{
    public RateLimitValidator()
    {
        RuleFor(x => x.Route)
            .NotEmpty().WithMessage("route cant be empty")
            .MaximumLength(100).WithMessage("route lenght cant be greater than 100")
            .MinimumLength(10).WithMessage("route lenght cant be less than 10");

        RuleFor(x => x.RequestsPerMinute)
            .GreaterThan(0).WithMessage("requests cant be < 0")
            .LessThanOrEqualTo(1000).WithMessage("Request per minutes must be less than or equal 1000");
    }
}