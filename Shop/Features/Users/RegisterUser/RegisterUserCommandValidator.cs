using FluentValidation;
using Shop.Domain.Enums;

namespace Shop.Features.Users.RegisterUser;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Login)
            .MinimumLength(2).WithMessage("Login must be at least 2 characters")
            .MaximumLength(64).WithMessage("Login must be at most 64 characters");

        RuleFor(c => c.Password)
            .MinimumLength(4).WithMessage("Password must be at least 4 characters");

        RuleFor(c => c.Username)
            .MinimumLength(2).WithMessage("Username must be at least 2 characters")
            .MaximumLength(64).WithMessage("Username must be at most 64 characters");

        RuleFor(c => c.Role)
            .NotEmpty().WithMessage("Role is required")
            .IsEnumName(typeof(Role), caseSensitive: false).WithMessage("Invalid role provided");
    }
}
