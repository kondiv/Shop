using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Infrastructure;

namespace Shop.Features.Items.DeleteItem;

internal sealed class DeleteItemCommandHandler(
    ApplicationContext context,
    ILogger<DeleteItemCommandHandler> logger): IRequestHandler<DeleteItemCommand, Result>
{
    public async Task<Result> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting item {id}", request.ItemId);

        var item = await context
            .Items
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.SellerId == request.SellerId, cancellationToken);

        if (item is null)
        {
            logger.LogError("Item with id {id} does not exist or user {userId} is not owner",
                request.ItemId, request.SellerId);
            return Result.NotFound();
        }

        context.Items.Remove(item);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Item deleted successfully");
        return Result.Success();
    }
}
