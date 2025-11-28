using Ardalis.Result;
using MediatR;
using Shop.Contracts.Item;

namespace Shop.Features.Items.UpdateItem;

internal sealed record UpdateItemCommand(int ItemId, Guid SellerId, UpdateItemRequest UpdateRequest) 
    : IRequest<Result>;
