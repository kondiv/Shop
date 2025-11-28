using Ardalis.Result;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts;
using Shop.Contracts.Item;
using Shop.Contracts.User;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Infrastructure;

namespace Shop.Features.Items.CreateItem;

internal sealed class CreateItemCommandHandler(
    ApplicationContext context,
    IValidator<CreateItemCommand> validator,
    ILogger<CreateItemCommandHandler> logger) 
    : IRequestHandler<CreateItemCommand, Result<ItemResponse>>
{
    public async Task<Result<ItemResponse>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating new item {createItemCommand}", request);

        var seller = await context
            .Users
            .Where(u => u.Id == request.SellerId && u.Role == Role.Seller)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (seller is null)
        {
            logger.LogError("User with id {id} does not exist or is not a seller", request.SellerId);
            return Result<ItemResponse>.Forbidden();
        }

        var commandValidationResult = validator.Validate(request);

        if (!commandValidationResult.IsValid)
        {
            logger.LogError("Invalid request for creating item. Errors\n{errors}", commandValidationResult.Errors);
            return Result<ItemResponse>.Invalid(
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

        var item = new Item
        {
            Name = request.Name,
            Price = request.Price,
            Quantity = request.Quantity,
            SellerId = request.SellerId,
            Category = Enum.Parse<Category>(request.Category, ignoreCase: true)
        };

        context.Items.Add(item);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Item created successfully");
        return Result<ItemResponse>.Success(new ItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Category = item.Category.ToString(),
            Price = item.Price,
            Quantity = item.Quantity,
            Seller = seller
        });
    }
}
