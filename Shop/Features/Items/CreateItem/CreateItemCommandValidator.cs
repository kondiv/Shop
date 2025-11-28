using FluentValidation;
using Shop.Domain.Enums;

namespace Shop.Features.Items.CreateItem;

internal sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(128).WithMessage("Name must be at most 128 characters");

        RuleFor(c => c.Price)
            .GreaterThan(0m).WithMessage("Price can not be negative");

        RuleFor(c => c.Category)
            .IsEnumName(typeof(Category), caseSensitive: false).WithMessage("Invalid category provided");

        RuleFor(c => c.Quantity)
            .GreaterThan(0).WithMessage("Item's quantity can not be negative");
    }
}
