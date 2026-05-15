using BicycleShop.Domain.Enums;

namespace BicycleShop.Application.DTOs.Orders;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal LoyaltyDiscountAmount { get; set; }
    public decimal BulkDiscountAmount { get; set; }
    public decimal PromotionDiscountAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
    public List<AppliedPromotionDto> AppliedPromotions { get; set; } = new();
    public List<OrderReservationResponseDto> Reservations { get; set; } = new();
}
