using Shop.Domain.Enums;

namespace Shop.Domain.Entities;

public sealed class Item
{
    public int Id { get; init; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public Category Category{ get; set; }

    public Guid SellerId { get; set; }

    public User Seller { get; set; } = null!;
}
