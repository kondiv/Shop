using FluentValidation;

namespace Shop.Features.Purchases.ListBuyerPurchases;

internal sealed class ListBuyerPurchasesRequestValidator : AbstractValidator<ListBuyerPurchasesRequest>
{
    public ListBuyerPurchasesRequestValidator()
    {
        RuleFor(r => r.Page)
            .GreaterThan(0).WithMessage("Page can not be negative");

        RuleFor(r => r.MaxPageSize)
            .GreaterThan(0).WithMessage("At least one element must be on the page");
    }
}
