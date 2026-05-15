namespace BicycleShop.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    
    public Guid CatalogId { get; set; }
    public Catalog Catalog { get; set; } = null!;
    public Inventory? Inventory { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();
}
