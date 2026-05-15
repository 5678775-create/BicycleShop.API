namespace BicycleShop.Domain.Entities;

using BicycleShop.Domain.Enums;

public class Order : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal LoyaltyDiscountAmount { get; set; }
    public decimal BulkDiscountAmount { get; set; }
    public decimal PromotionDiscountAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<AppliedPromotion> AppliedPromotions { get; set; } = new List<AppliedPromotion>();
    public ICollection<InventoryReservation> Reservations { get; set; } = new List<InventoryReservation>();
}
