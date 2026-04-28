using FluentValidation;
using UserService.Domain;

namespace UserService.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("login is required")
                .MaximumLength(50);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("password is required")
                .MinimumLength(6);

            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Surname).NotEmpty();
            RuleFor(x => x.Age).InclusiveBetween(0, 150);
        });

        RuleSet("Update", () =>
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("password is required")
                .MinimumLength(6);

            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Surname).NotEmpty();
            RuleFor(x => x.Age).InclusiveBetween(0, 150);
        });
    }
}
