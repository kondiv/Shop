using FluentValidation;
using Shop.Domain.Enums;

namespace Shop.Features.Items.ListItems;

internal sealed class ListItemsRequestValidator : AbstractValidator<ListItemsRequest>
{
    public ListItemsRequestValidator()
    {
        RuleFor(r => r.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(r => r.MaxPageSize)
            .GreaterThan(0).WithMessage("At least one element required");

        RuleFor(r => r.Category)
            .IsEnumName(typeof(Category), caseSensitive: false).WithMessage("Invalid category")
            .When(r => !string.IsNullOrEmpty(r.Category));
    }
}
