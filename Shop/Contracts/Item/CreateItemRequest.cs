namespace Shop.Contracts.Item;

public sealed class CreateItemRequest
{
    public required string Name { get; init; }
    
    public required string Category { get; init; }
    
    public required int Quantity { get; init; }
    
    public required decimal Price { get; init; }
}
