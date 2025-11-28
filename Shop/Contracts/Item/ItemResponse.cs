using Shop.Contracts.User;

namespace Shop.Contracts.Item;

public sealed class ItemResponse
{
    public required int Id { get; init; }
    
    public required string Name { get; init; }

    public required string Category { get; init; }
    
    public required int Quantity { get; init; }
    
    public required decimal Price { get; init; }
    
    public required UserResponse Seller { get; init; }
}
