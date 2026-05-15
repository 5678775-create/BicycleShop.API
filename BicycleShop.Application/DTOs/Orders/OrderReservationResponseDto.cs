namespace BicycleShop.Application.DTOs.Orders;

public class OrderReservationResponseDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsReleased { get; set; }
    public bool IsCommitted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
}
