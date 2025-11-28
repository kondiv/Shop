using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Contracts.Item;
using Shop.Features.Items.CreateItem;
using Shop.Features.Items.DeleteItem;
using Shop.Features.Items.GetItem;
using Shop.Features.Items.ListItems;
using Shop.Features.Items.UpdateItem;
using System.Security.Claims;

namespace Shop.Controllers;

[ApiController]
[Route("api/items/")]
public class ItemsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemResponse>> GetAsync([FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        var request = new GetItemRequest(id);

        var getItemResult = await mediator.Send(request, cancellationToken);

        return getItemResult.ToActionResult(this);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ItemResponse>>> ListAsync(
        [FromQuery] int page = 1,
        [FromQuery] int maxPageSize = 10,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ListItemsRequest(page, maxPageSize, category);

        var listItemsResult = await mediator.Send(request, cancellationToken);

        return listItemsResult.ToActionResult(this);
    }

    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<ActionResult<ItemResponse>> CreateAsync(
        CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var sellerId))
        {
            return Unauthorized("User Id not found in token");
        }

        var command = new CreateItemCommand(
            request.Name,
            request.Price, 
            request.Category,
            request.Quantity, 
            sellerId);

        var createItemResult = await mediator.Send(command, cancellationToken);

        return createItemResult.ToActionResult(this);
    }

    [Authorize(Roles = "Seller", Policy = "ItemOwner")]
    [HttpPatch("{id:int}")]
    public async Task<ActionResult> UpdateAsync(
        [FromRoute] int id,
        UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var sellerId))
        {
            return Unauthorized("User Id not found in token");
        }

        var command = new UpdateItemCommand(id, sellerId, request);

        var updateItemResult = await mediator.Send(command, cancellationToken);

        return updateItemResult.ToActionResult(this);
    }

    [Authorize(Roles = "Seller", Policy = "ItemOwner")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync(
        [FromRoute] int id, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var sellerId))
        {
            return Unauthorized("User Id not found in token");
        }

        var command = new DeleteItemCommand(id, sellerId);

        var deleteItemResult = await mediator.Send(command, cancellationToken);

        return deleteItemResult.ToActionResult(this);
    }
}
