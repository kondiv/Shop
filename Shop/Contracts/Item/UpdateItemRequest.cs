namespace Shop.Contracts.Item;

public sealed class UpdateItemRequest
{
    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public string? Category { get; set; }
}
