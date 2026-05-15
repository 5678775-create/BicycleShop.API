using BicycleShop.Application.DTOs.Inventory;
using BicycleShop.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BicycleShop.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>Gets inventory records for all products.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InventoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _inventoryService.GetAllAsync());

    /// <summary>Gets inventory by product id.</summary>
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(InventoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProductId(Guid productId) =>
        Ok(await _inventoryService.GetInventoryByProductIdAsync(productId));

    /// <summary>Updates available inventory for a product.</summary>
    [HttpPatch("{productId:guid}")]
    [ProducesResponseType(typeof(InventoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid productId, UpdateInventoryRequestDto request) =>
        Ok(await _inventoryService.UpdateInventoryAsync(productId, request.QuantityAvailable));
}
