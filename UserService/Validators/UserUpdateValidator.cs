using FluentValidation;
using UserService.Domain;

namespace UserService.Validators;

public class UserUpdateValidator : AbstractValidator<User>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("password is required")
            .MinimumLength(6);

        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Surname).NotEmpty();
        RuleFor(x => x.Age).InclusiveBetween(0, 150);
    }
}
