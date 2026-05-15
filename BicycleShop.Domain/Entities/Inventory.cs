namespace BicycleShop.Domain.Entities;

public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int AvailableForReservation => QuantityAvailable - QuantityReserved;
}
