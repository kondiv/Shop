using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts.Item;
using Shop.Contracts.User;
using Shop.Infrastructure;

namespace Shop.Features.Items.GetItem;

internal sealed class GetItemRequestHandler(
    ApplicationContext context,
    ILogger<GetItemRequestHandler> logger)
    : IRequestHandler<GetItemRequest, Result<ItemResponse>>
{
    public async Task<Result<ItemResponse>> Handle(GetItemRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Requesting item with id {id}", request.Id);

        var item = await context
            .Items
            .Select(i => new ItemResponse
            {
                Id = i.Id,
                Name = i.Name,
                Category = i.Category.ToString(),
                Price = i.Price,
                Quantity = i.Quantity,
                Seller = new UserResponse
                {
                    Id = i.Seller.Id,
                    Username = i.Seller.Username,
                    Role = i.Seller.Role.ToString()
                }
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item is null)
        {
            logger.LogError("Item with id {id} does not exist", request.Id);
            return Result<ItemResponse>.NotFound("Item not found");
        }

        logger.LogInformation("Item successfully found {item}", item);

        return Result<ItemResponse>.Success(item);
    }
}
