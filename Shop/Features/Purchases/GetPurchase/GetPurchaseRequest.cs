using Ardalis.Result;
using MediatR;
using Shop.Contracts.Purchase;

namespace Shop.Features.Purchases.GetPurchase;

internal sealed record GetPurchaseRequest(Guid Id) : IRequest<Result<PurchaseResponse>>;
