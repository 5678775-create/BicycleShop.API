using BicycleShop.Application.DTOs.Promotions;
using BicycleShop.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BicycleShop.API.Controllers;

[ApiController]
[Route("api/promotions")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    /// <summary>Gets all promotions.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromotionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _promotionService.GetAllAsync());

    /// <summary>Gets promotions active at the current UTC time.</summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<PromotionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive() =>
        Ok(await _promotionService.GetActiveAsync(DateTime.UtcNow));

    /// <summary>Creates a promotion.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PromotionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreatePromotionRequestDto request)
    {
        var created = await _promotionService.CreateAsync(request);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}
