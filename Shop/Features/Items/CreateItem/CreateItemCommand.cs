using Ardalis.Result;
using MediatR;
using Shop.Contracts.Item;

namespace Shop.Features.Items.CreateItem;

internal sealed record CreateItemCommand(
    string Name,
    decimal Price,
    string Category,
    int Quantity,
    Guid SellerId) 
    : IRequest<Result<ItemResponse>>;
