namespace Shop.Contracts.Purchase;

public sealed class PurchaseCreateRequest
{
    public required int ItemId { get; init; }

    public required int Quantity { get; init; }
}
