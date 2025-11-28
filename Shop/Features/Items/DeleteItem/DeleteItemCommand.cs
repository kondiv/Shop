using Ardalis.Result;
using MediatR;

namespace Shop.Features.Items.DeleteItem;

internal sealed record DeleteItemCommand(int ItemId, Guid SellerId) : IRequest<Result>;

