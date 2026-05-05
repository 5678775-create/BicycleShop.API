namespace BicycleShop.Application.DTOs;

public record ProductDto(Guid Id, string Name, decimal Price, int StockQuantity);

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid CatalogId { get; set; }
}