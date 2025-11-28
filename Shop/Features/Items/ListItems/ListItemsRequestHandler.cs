using Ardalis.Result;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts.Item;
using Shop.Contracts.User;
using Shop.Domain.Enums;
using Shop.Infrastructure;

namespace Shop.Features.Items.ListItems;

internal sealed class ListItemsRequestHandler(
    ApplicationContext context,
    IValidator<ListItemsRequest> validator,
    ILogger<ListItemsRequestHandler> logger)
    : IRequestHandler<ListItemsRequest, Result<IReadOnlyCollection<ItemResponse>>>
{
    public async Task<Result<IReadOnlyCollection<ItemResponse>>> Handle(ListItemsRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("List items request\nPage {p}\nMax page size {mps}\nCategory {c}",
            request.Page, request.MaxPageSize, request.Category);

        var requestValidationResult = validator.Validate(request);

        if (!requestValidationResult.IsValid)
        {
            logger.LogError("Invalid request: {errors}", requestValidationResult.Errors);
            return Result<IReadOnlyCollection<ItemResponse>>.Invalid(
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

        var query = context
            .Items
            .AsQueryable();

        if (request.Category is not null)
        {
            var category = Enum.Parse<Category>(request.Category, ignoreCase: true);

            query = query
                .Where(i => i.Category == category);
        }

        var items = await query
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
            .OrderBy(i => i.Id)
            .Skip((request.Page - 1) * request.MaxPageSize)
            .Take(request.MaxPageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Listing completed");
        return Result<IReadOnlyCollection<ItemResponse>>.Success(items);
    }
}
