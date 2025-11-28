using Shop.Contracts.User;

namespace Shop.Contracts.Purchase;

public class PurchaseResponse
{
    public required Guid Id { get; init; }

    public required int Quantity { get; init; }

    public required decimal TotalPrice { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required UserResponse Seller { get; init; }
    
    public required UserResponse Buyer { get; init; }
    
    public required PurchaseItemResponse Item { get; init; }
}
