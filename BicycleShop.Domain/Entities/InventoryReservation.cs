namespace BicycleShop.Domain.Entities;

public class InventoryReservation : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; set; }
    public bool IsReleased { get; set; }
    public bool IsCommitted { get; set; }
}
