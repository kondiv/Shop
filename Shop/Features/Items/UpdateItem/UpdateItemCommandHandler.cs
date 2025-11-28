using Ardalis.Result;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Enums;
using Shop.Infrastructure;
using System.Data;

namespace Shop.Features.Items.UpdateItem;

internal sealed class UpdateItemCommandHandler(
    ApplicationContext context,
    IValidator<UpdateItemCommand> validator,
    ILogger<UpdateItemCommandHandler> logger) : IRequestHandler<UpdateItemCommand, Result>
{
    public async Task<Result> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating item {id} with data: {updateData}", request.ItemId, request.UpdateRequest);

        var item = await context
            .Items
            .FirstOrDefaultAsync(u => u.Id == request.ItemId && u.SellerId == request.SellerId, cancellationToken);

        if (item is null)
        {
            logger.LogError("Item with id {id} not found or it does not belong to user {userId}",
                request.ItemId, request.SellerId);
            return Result.NotFound();
        }

        var commandValidationResult = validator.Validate(request);

        if (!commandValidationResult.IsValid)
        {
            logger.LogError("Invalid data provided. Errors\n{errors}", commandValidationResult.Errors);
            return Result.Invalid(
                commandValidationResult
                    .Errors
                    .Select(e => new ValidationError
                    {
                        ErrorCode = e.ErrorCode,
                        ErrorMessage = e.ErrorMessage,
                    })
                    .ToArray()
            );
        }

        item.Name = request.UpdateRequest.Name ?? item.Name;
        item.Price = request.UpdateRequest.Price ?? item.Price;
        item.Category = request.UpdateRequest.Category is null ?
            item.Category
            : Enum.Parse<Category>(request.UpdateRequest.Category);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Item {id} updated successfully", request.ItemId);

        return Result.Success();
    }
}
