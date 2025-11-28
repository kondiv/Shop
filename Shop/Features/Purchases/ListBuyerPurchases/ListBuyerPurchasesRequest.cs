using Ardalis.Result;
using MediatR;
using Shop.Contracts.Purchase;

namespace Shop.Features.Purchases.ListBuyerPurchases;

internal sealed record ListBuyerPurchasesRequest(
    Guid BuyerId,
    int Page,
    int MaxPageSize)
    : IRequest<Result<IReadOnlyCollection<PurchaseResponse>>>;
