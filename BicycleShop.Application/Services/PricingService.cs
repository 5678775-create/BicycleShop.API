using BicycleShop.Application.DTOs.Orders;
using BicycleShop.Application.DTOs.Pricing;
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;

namespace BicycleShop.Application.Services;

public class PricingService : IPricingService
{
    public Task<PriceCalculationResultDto> CalculateAsync(
        Customer customer,
        IReadOnlyCollection<OrderPricingItem> items,
        IReadOnlyCollection<Promotion> promotions,
        DateTime now)
    {
        var subtotal = RoundMoney(items.Sum(i => i.UnitPrice * i.Quantity));
        var loyaltyDiscount = RoundMoney(subtotal * GetLoyaltyDiscountPercent(customer.LoyaltyTier));
        var bulkDiscount = CalculateBulkDiscount(subtotal, items.Sum(i => i.Quantity));
        var appliedPromotions = CalculatePromotionDiscounts(items, promotions, now);
        var promotionDiscount = RoundMoney(appliedPromotions.Sum(p => p.DiscountAmount));
        var totalDiscount = RoundMoney(loyaltyDiscount + bulkDiscount + promotionDiscount);
        var finalAmount = Math.Max(0m, RoundMoney(subtotal - totalDiscount));

        return Task.FromResult(new PriceCalculationResultDto
        {
            Subtotal = subtotal,
            LoyaltyDiscountAmount = loyaltyDiscount,
            BulkDiscountAmount = bulkDiscount,
            PromotionDiscountAmount = promotionDiscount,
            TotalDiscountAmount = totalDiscount,
            FinalAmount = finalAmount,
            AppliedPromotions = appliedPromotions
        });
    }

    private static decimal GetLoyaltyDiscountPercent(CustomerLoyaltyTier tier) =>
        tier switch
        {
            CustomerLoyaltyTier.Bronze => 0.02m,
            CustomerLoyaltyTier.Silver => 0.05m,
            CustomerLoyaltyTier.Gold => 0.10m,
            _ => 0m
        };

    private static decimal CalculateBulkDiscount(decimal subtotal, int totalQuantity)
    {
        var quantityDiscount = totalQuantity >= 3 ? subtotal * 0.05m : 0m;
        var subtotalDiscount = subtotal >= 50000m ? subtotal * 0.07m : 0m;

        return RoundMoney(Math.Max(quantityDiscount, subtotalDiscount));
    }

    private static List<AppliedPromotionDto> CalculatePromotionDiscounts(
        IReadOnlyCollection<OrderPricingItem> items,
        IReadOnlyCollection<Promotion> promotions,
        DateTime now)
    {
        var appliedPromotions = new List<AppliedPromotionDto>();

        foreach (var promotion in promotions.Where(p => p.IsActive && p.ActiveFrom <= now && p.ActiveTo >= now))
        {
            var discountBase = promotion.Type switch
            {
                PromotionType.TimeBased => items.Sum(i => i.UnitPrice * i.Quantity),
                PromotionType.CategoryBased when promotion.CatalogId.HasValue => items
                    .Where(i => i.CatalogId == promotion.CatalogId.Value)
                    .Sum(i => i.UnitPrice * i.Quantity),
                _ => 0m
            };

            var discountAmount = RoundMoney(discountBase * (promotion.DiscountPercent / 100m));
            if (discountAmount <= 0m)
            {
                continue;
            }

            appliedPromotions.Add(new AppliedPromotionDto
            {
                PromotionId = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                DiscountAmount = discountAmount
            });
        }

        return appliedPromotions;
    }

    private static decimal RoundMoney(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
