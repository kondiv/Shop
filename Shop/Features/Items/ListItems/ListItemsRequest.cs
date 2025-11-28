using Ardalis.Result;
using MediatR;
using Shop.Contracts.Item;

namespace Shop.Features.Items.ListItems;

internal sealed record ListItemsRequest(int Page, int MaxPageSize, string? Category) 
    : IRequest<Result<IReadOnlyCollection<ItemResponse>>>;

