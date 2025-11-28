namespace Shop.Domain.Entities;

public sealed class Purchase
{
    public Guid Id { get; init; }

    public Guid BuyerId { get; set; }

    public User Buyer { get; set; } = null!;

    public Guid SellerId { get; set; }

    public User Seller { get; set; } = null!;

    public int ItemId { get; set; }

    public Item Item { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;
}
