namespace BicycleShop.Application.DTOs;

public record CatalogDto(Guid Id, string Name, string Description);

public class CreateCatalogDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}