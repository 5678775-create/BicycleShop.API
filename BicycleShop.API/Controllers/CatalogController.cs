using BicycleShop.Application.DTOs;
using BicycleShop.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BicycleShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _service;

    public CatalogController(ICatalogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var catalog = await _service.GetByIdAsync(id);
        return catalog != null ? Ok(catalog) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCatalogDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreateCatalogDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}