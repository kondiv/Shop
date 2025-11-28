using Ardalis.Result;
using MediatR;
using Shop.Contracts.Purchase;

namespace Shop.Features.Purchases.CreatePurchase;

internal sealed record CreatePurchaseCommand(int ItemId, Guid BuyerId, int Quantity)
    : IRequest<Result<PurchaseResponse>>;
