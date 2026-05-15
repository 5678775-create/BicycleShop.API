using BicycleShop.Application.DTOs.Orders;

namespace BicycleShop.Application.DTOs.Pricing;

public class PriceCalculationResultDto
{
    public decimal Subtotal { get; set; }
    public decimal LoyaltyDiscountAmount { get; set; }
    public decimal BulkDiscountAmount { get; set; }
    public decimal PromotionDiscountAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public List<AppliedPromotionDto> AppliedPromotions { get; set; } = new();
}
