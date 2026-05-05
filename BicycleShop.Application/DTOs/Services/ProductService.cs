using BicycleShop.Application.DTOs;
using BicycleShop.Application.Services;
using BicycleShop.Data.Repositories;
using BicycleShop.Domain.Entities;

namespace BicycleShop.Application.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;

    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _repository.GetAllAsync();
        return products.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.StockQuantity));
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var p = await _repository.GetByIdAsync(id);
        return p == null ? null : new ProductDto(p.Id, p.Name, p.Price, p.StockQuantity);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name, Description = dto.Description, 
            Price = dto.Price, StockQuantity = dto.StockQuantity, CatalogId = dto.CatalogId
        };
        await _repository.AddAsync(product);
        return new ProductDto(product.Id, product.Name, product.Price, product.StockQuantity);
    }

    public async Task UpdateAsync(Guid id, CreateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return;
        
        product.Name = dto.Name;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CatalogId = dto.CatalogId;
        await _repository.UpdateAsync(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product != null) await _repository.DeleteAsync(product);
    }
}