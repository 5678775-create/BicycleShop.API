namespace BicycleShop.Application.DTOs.Pricing;

public class OrderPricingItem
{
    public Guid ProductId { get; set; }
    public Guid CatalogId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
