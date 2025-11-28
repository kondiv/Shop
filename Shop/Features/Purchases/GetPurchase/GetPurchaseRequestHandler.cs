using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts.Purchase;
using Shop.Contracts.User;
using Shop.Infrastructure;

namespace Shop.Features.Purchases.GetPurchase;

internal sealed class GetPurchaseRequestHandler(
    ApplicationContext context,
    ILogger<GetPurchaseRequestHandler> logger)
    : IRequestHandler<GetPurchaseRequest, Result<PurchaseResponse>>
{
    public async Task<Result<PurchaseResponse>> Handle(GetPurchaseRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Requesting purchase {id}", request.Id);

        var purchase = await context
            .Purchases
            .Select(p => new PurchaseResponse
            {
                Id = p.Id,
                TotalPrice = p.TotalPrice,
                Quantity = p.Quantity,
                CreatedAtUtc = p.CreatedAtUtc,
                Item = new PurchaseItemResponse
                {
                    Id = p.Item.Id,
                    Name = p.Item.Name,
                    Price = p.Item.Price
                },
                Seller = new UserResponse
                {
                    Id = p.Seller.Id,
                    Username = p.Seller.Username,
                    Role = p.Seller.Role.ToString()
                },
                Buyer = new UserResponse
                {
                    Id = p.Buyer.Id,
                    Username = p.Buyer.Username,
                    Role = p.Buyer.Role.ToString()
                }
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (purchase is null)
        {
            logger.LogError("Purchase with id {id} not found", request.Id);
            return Result<PurchaseResponse>.NotFound();
        }

        logger.LogInformation("Found purchase {id}", request.Id);
        return Result<PurchaseResponse>.Success(purchase);
    }
}
