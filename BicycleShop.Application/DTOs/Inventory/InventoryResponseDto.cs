namespace BicycleShop.Application.DTOs.Inventory;

public class InventoryResponseDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityAvailableForReservation { get; set; }
    public DateTime UpdatedAt { get; set; }
}
