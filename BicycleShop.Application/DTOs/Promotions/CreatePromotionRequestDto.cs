using BicycleShop.Domain.Enums;

namespace BicycleShop.Application.DTOs.Promotions;

public class CreatePromotionRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveTo { get; set; }
    public Guid? CatalogId { get; set; }
}
