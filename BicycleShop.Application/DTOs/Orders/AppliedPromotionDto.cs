namespace BicycleShop.Application.DTOs.Orders;

public class AppliedPromotionDto
{
    public Guid? PromotionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
}
