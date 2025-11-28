using Ardalis.Result;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts.Purchase;
using Shop.Contracts.User;
using Shop.Domain.Enums;
using Shop.Infrastructure;

namespace Shop.Features.Purchases.ListBuyerPurchases;

internal sealed class ListBuyerPurchasesRequestHandler(
    ApplicationContext context,
    IValidator<ListBuyerPurchasesRequest> validator,
    ILogger<ListBuyerPurchasesRequestHandler> logger)
    : IRequestHandler<ListBuyerPurchasesRequest, Result<IReadOnlyCollection<PurchaseResponse>>>
{
    public async Task<Result<IReadOnlyCollection<PurchaseResponse>>> Handle(ListBuyerPurchasesRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Requesting user's ({id}) purchases", request.BuyerId);

        var buyer = await context
            .Users
            .Where(u => u.Role == Role.Buyer)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role.ToString()
            })
            .FirstOrDefaultAsync(u => u.Id == request.BuyerId, cancellationToken);

        if (buyer is null)
        {
            logger.LogError("User {id} not found", request.BuyerId);
            return Result<IReadOnlyCollection<PurchaseResponse>>.NotFound("User not found");
        }

        var requestValidationResult = validator.Validate(request);

        if (!requestValidationResult.IsValid)
        {
            logger.LogError("Invalid data provided. Errors\n{errors}", requestValidationResult.Errors);
            return Result<IReadOnlyCollection<PurchaseResponse>>.Invalid(
                requestValidationResult
                .Errors
                .Select(e => new ValidationError
                {
                    ErrorCode = e.ErrorCode,
                    ErrorMessage = e.ErrorMessage
                })
                .ToArray()
            );
        }

        var purchases = await context
            .Purchases
            .Select(p => new PurchaseResponse
            {
                Id = p.Id,
                TotalPrice = p.TotalPrice,
                CreatedAtUtc = p.CreatedAtUtc,
                Quantity = p.Quantity,
                Item = new PurchaseItemResponse
                {
                    Id = p.Item.Id,
                    Name = p.Item.Name,
                    Price = p.Item.Price
                },
                Buyer = buyer,
                Seller = new UserResponse
                {
                    Id = p.Seller.Id,
                    Username = p.Seller.Username,
                    Role = p.Seller.Role.ToString()
                }
            })
            .OrderBy(p => p.Id)
            .Skip((request.Page - 1) * request.MaxPageSize)
            .Take(request.MaxPageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        logger.LogInformation("Listing completed");
        return Result<IReadOnlyCollection<PurchaseResponse>>.Success(purchases);
    }
}
