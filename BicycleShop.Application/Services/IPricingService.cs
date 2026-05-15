using BicycleShop.Application.DTOs.Pricing;
using BicycleShop.Domain.Entities;

namespace BicycleShop.Application.Services;

public interface IPricingService
{
    Task<PriceCalculationResultDto> CalculateAsync(
        Customer customer,
        IReadOnlyCollection<OrderPricingItem> items,
        IReadOnlyCollection<Promotion> promotions,
        DateTime now);
}
