using FluentValidation;
using Shop.Domain.Enums;

namespace Shop.Features.Items.UpdateItem;

internal sealed class UpdateItemCommandValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemCommandValidator()
    {
        RuleFor(c => c.UpdateRequest.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(128).WithMessage("Name must be at most 128")
            .When(c => !string.IsNullOrEmpty(c.UpdateRequest.Name));

        RuleFor(c => c.UpdateRequest.Price)
            .GreaterThan(0m).WithMessage("Price can not be negative")
            .When(c => c.UpdateRequest.Price is not null);

        RuleFor(c => c.UpdateRequest.Category)
            .IsEnumName(typeof(Category), caseSensitive: false).WithMessage("Invalid category")
            .When(c => c.UpdateRequest.Category is not null);
    }
}
