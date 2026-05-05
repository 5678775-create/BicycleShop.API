using BicycleShop.Application.DTOs;
using BicycleShop.Data.Repositories;
using BicycleShop.Domain.Entities;

namespace BicycleShop.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly IRepository<Catalog> _repository;

    public CatalogService(IRepository<Catalog> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CatalogDto>> GetAllAsync()
    {
        var catalogs = await _repository.GetAllAsync();
        return catalogs.Select(c => new CatalogDto(c.Id, c.Name, c.Description));
    }

    public async Task<CatalogDto?> GetByIdAsync(Guid id)
    {
        var c = await _repository.GetByIdAsync(id);
        return c == null ? null : new CatalogDto(c.Id, c.Name, c.Description);
    }

    public async Task<CatalogDto> CreateAsync(CreateCatalogDto dto)
    {
        var catalog = new Catalog
        {
            Name = dto.Name,
            Description = dto.Description
        };
        
        await _repository.AddAsync(catalog);
        return new CatalogDto(catalog.Id, catalog.Name, catalog.Description);
    }

    public async Task UpdateAsync(Guid id, CreateCatalogDto dto)
    {
        var catalog = await _repository.GetByIdAsync(id);
        if (catalog == null) return;
        
        catalog.Name = dto.Name;
        catalog.Description = dto.Description;
        
        await _repository.UpdateAsync(catalog);
    }

    public async Task DeleteAsync(Guid id)
    {
        var catalog = await _repository.GetByIdAsync(id);
        if (catalog != null) await _repository.DeleteAsync(catalog);
    }
}