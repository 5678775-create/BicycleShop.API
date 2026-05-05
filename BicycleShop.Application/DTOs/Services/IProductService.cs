using BicycleShop.Application.DTOs;

namespace BicycleShop.Application.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task UpdateAsync(Guid id, CreateProductDto dto);
    Task DeleteAsync(Guid id);
}