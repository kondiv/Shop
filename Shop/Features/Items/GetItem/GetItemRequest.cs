using Ardalis.Result;
using MediatR;
using Shop.Contracts.Item;

namespace Shop.Features.Items.GetItem;

internal sealed record GetItemRequest(int Id) : IRequest<Result<ItemResponse>>;