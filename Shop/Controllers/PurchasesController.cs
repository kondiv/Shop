using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Contracts.Purchase;
using Shop.Features.Purchases.CreatePurchase;
using Shop.Features.Purchases.GetPurchase;
using Shop.Features.Purchases.ListBuyerPurchases;
using System.Security.Claims;

namespace Shop.Controllers;

[ApiController]
[Route("api/purchases/")]
public class PurchasesController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PurchaseResponse>> CreateAsync(
        PurchaseCreateRequest request, CancellationToken cancellationToken = default)
    {
        var buyerId = GetCurrentUserId();

        if (buyerId is null)
        {
            return Unauthorized();
        }

        var command = new CreatePurchaseCommand(request.ItemId, buyerId.Value, request.Quantity);

        var createPurchaseResult = await mediator.Send(command, cancellationToken);

        return createPurchaseResult.ToActionResult(this);
    }

    [Authorize(Roles = "Buyer")]
    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyCollection<PurchaseResponse>>> ListBuyerHistoryAsync(
        int page = 1,
        int maxPageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var buyerId = GetCurrentUserId();

        if (buyerId is null)
        {
            return Unauthorized();
        }

        var request = new ListBuyerPurchasesRequest(buyerId.Value, page, maxPageSize);

        var listPurchasesResult = await mediator.Send(request, cancellationToken);

        return listPurchasesResult.ToActionResult(this);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseResponse>> GetAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPurchaseRequest(id);

        var getPurchaseResult = await mediator.Send(request, cancellationToken);

        return getPurchaseResult.ToActionResult(this);
    }


    private Guid? GetCurrentUserId()
    {
        var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userClaimId is null || !Guid.TryParse(userClaimId.Value, out var userId))
        {
            return null;
        }

        return userId;
    }
}
