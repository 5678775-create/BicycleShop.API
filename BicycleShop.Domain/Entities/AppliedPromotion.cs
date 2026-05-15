namespace BicycleShop.Domain.Entities;

public class AppliedPromotion : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? PromotionId { get; set; }
    public Promotion? Promotion { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
}
