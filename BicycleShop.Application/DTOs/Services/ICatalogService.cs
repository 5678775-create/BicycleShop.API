using BicycleShop.Application.DTOs;

namespace BicycleShop.Application.Services;

public interface ICatalogService
{
    Task<IEnumerable<CatalogDto>> GetAllAsync();
    Task<CatalogDto?> GetByIdAsync(Guid id);
    Task<CatalogDto> CreateAsync(CreateCatalogDto dto);
    Task UpdateAsync(Guid id, CreateCatalogDto dto);
    Task DeleteAsync(Guid id);
}