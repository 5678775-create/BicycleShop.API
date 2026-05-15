namespace BicycleShop.Domain.Entities;

using BicycleShop.Domain.Enums;

public class Promotion : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveTo { get; set; }
    public Guid? CatalogId { get; set; }
    public Catalog? Catalog { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<AppliedPromotion> AppliedPromotions { get; set; } = new List<AppliedPromotion>();
}
