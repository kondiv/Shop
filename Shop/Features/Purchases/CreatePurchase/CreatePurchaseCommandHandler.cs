using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts.Purchase;
using Shop.Contracts.User;
using Shop.Domain.Entities;
using Shop.Infrastructure;

namespace Shop.Features.Purchases.CreatePurchase;

internal sealed class CreatePurchaseCommandHandler(
    ApplicationContext context,
    ILogger<CreatePurchaseCommandHandler> logger) : IRequestHandler<CreatePurchaseCommand, Result<PurchaseResponse>>
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    
    public async Task<Result<PurchaseResponse>> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating new purchase {request}", request);

        await _lock.WaitAsync(cancellationToken);
        
        try
        {
            var item = await context
                .Items
                .Include(i => i.Seller)
                .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

            if (item is null)
            {
                logger.LogError("Item with id {id} not found", request.ItemId);
                return Result<PurchaseResponse>.NotFound("Item not found");
            }

            if (item.SellerId == request.BuyerId)
            {
                logger.LogError("Attempt to buy own product");
                return Result<PurchaseResponse>.Error("Attempt to buy own product");
            }

            if (item.Quantity < request.Quantity)
            {
                logger.LogError("Not enough items. Requested quantity - {rq}, actual quantity - {aq}",
                    request.Quantity, item.Quantity);
                return Result<PurchaseResponse>.Error("Not enough items");
            }

            var buyer = await context
                .Users
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Role = u.Role.ToString(),
                    Username = u.Username
                })
                .FirstOrDefaultAsync(u => u.Id == request.BuyerId, cancellationToken);

            if (buyer is null)
            {
                logger.LogError("User with id {id} does not exist", request.BuyerId);
                return Result<PurchaseResponse>.Unauthorized();
            }

            item.Quantity -= request.Quantity;

            var purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                ItemId = request.ItemId,
                BuyerId = buyer.Id,
                SellerId = item.SellerId,
                Quantity = request.Quantity,
                TotalPrice = item.Price * request.Quantity
            };

            context.Purchases.Add(purchase);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Purchase created");
            return Result<PurchaseResponse>.Created(new PurchaseResponse
            {
                Id = purchase.Id,
                Quantity = purchase.Quantity,
                TotalPrice = purchase.TotalPrice,
                CreatedAtUtc = purchase.CreatedAtUtc,
                Item = new PurchaseItemResponse
                {
                    Id = item.Id,
                    Price = item.Price,
                    Name = item.Name
                },
                Buyer = buyer,
                Seller = new UserResponse
                {
                    Id = item.SellerId,
                    Role = item.Seller.Role.ToString(),
                    Username = item.Seller.Username
                }
            });
        }
        finally
        {
            _lock.Release();
        }
    }
}
