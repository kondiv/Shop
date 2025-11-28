namespace Shop.Contracts.Purchase;

public sealed class PurchaseItemResponse
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required decimal Price { get; init; }
}
