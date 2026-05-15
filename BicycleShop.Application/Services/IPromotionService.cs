using BicycleShop.Application.DTOs.Promotions;
using BicycleShop.Domain.Entities;

namespace BicycleShop.Application.Services;

public interface IPromotionService
{
    Task<IEnumerable<PromotionResponseDto>> GetAllAsync();
    Task<IEnumerable<PromotionResponseDto>> GetActiveAsync(DateTime now);
    Task<IReadOnlyCollection<Promotion>> GetActiveEntitiesAsync(DateTime now);
    Task<PromotionResponseDto> CreateAsync(CreatePromotionRequestDto request);
}
