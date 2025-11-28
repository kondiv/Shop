namespace Shop.Domain.Models;

public sealed class CategoryStatistics
{
    public string MostSellableCategory { get; set; } = string.Empty;
    
    public decimal AverageCategoryPrice { get; set; }
}
