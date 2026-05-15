using BicycleShop.Application.Services;
using BicycleShop.Application.DTOs.Pricing;
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;

namespace BicycleShop.Tests;

public class PricingServiceTests
{
    private readonly PricingService _pricingService = new();
    private static readonly Guid BikesCatalogId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AccessoriesCatalogId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task BronzeCustomer_GetsCorrectLoyaltyDiscount()
    {
        var result = await CalculateAsync(CustomerLoyaltyTier.Bronze, [Item(1000m, 1, BikesCatalogId)]);

        Assert.Equal(20m, result.LoyaltyDiscountAmount);
        Assert.Equal(980m, result.FinalAmount);
    }

    [Fact]
    public async Task SilverCustomer_GetsCorrectLoyaltyDiscount()
    {
        var result = await CalculateAsync(CustomerLoyaltyTier.Silver, [Item(1000m, 1, BikesCatalogId)]);

        Assert.Equal(50m, result.LoyaltyDiscountAmount);
        Assert.Equal(950m, result.FinalAmount);
    }

    [Fact]
    public async Task GoldCustomer_GetsCorrectLoyaltyDiscount()
    {
        var result = await CalculateAsync(CustomerLoyaltyTier.Gold, [Item(1000m, 1, BikesCatalogId)]);

        Assert.Equal(100m, result.LoyaltyDiscountAmount);
        Assert.Equal(900m, result.FinalAmount);
    }

    [Fact]
    public async Task BulkDiscount_AppliesWhenTotalQuantityIsAtLeastThree()
    {
        var result = await CalculateAsync(CustomerLoyaltyTier.Bronze, [Item(100m, 3, BikesCatalogId)]);

        Assert.Equal(15m, result.BulkDiscountAmount);
    }

    [Fact]
    public async Task BulkDiscount_AppliesWhenSubtotalIsAtLeastFiftyThousand()
    {
        var result = await CalculateAsync(CustomerLoyaltyTier.Bronze, [Item(60000m, 1, BikesCatalogId)]);

        Assert.Equal(4200m, result.BulkDiscountAmount);
    }

    [Fact]
    public async Task TimeBasedPromotion_AppliesOnlyInActivePeriod()
    {
        var now = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);
        var promotion = Promotion(PromotionType.TimeBased, 10m, now.AddDays(-1), now.AddDays(1));

        var result = await CalculateAsync(CustomerLoyaltyTier.Bronze, [Item(1000m, 1, BikesCatalogId)], [promotion], now);

        Assert.Equal(100m, result.PromotionDiscountAmount);
        Assert.Single(result.AppliedPromotions);
    }

    [Fact]
    public async Task TimeBasedPromotion_DoesNotApplyOutsideActivePeriod()
    {
        var now = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);
        var promotion = Promotion(PromotionType.TimeBased, 10m, now.AddDays(-10), now.AddDays(-1));

        var result = await CalculateAsync(CustomerLoyaltyTier.Bronze, [Item(1000m, 1, BikesCatalogId)], [promotion], now);

        Assert.Equal(0m, result.PromotionDiscountAmount);
        Assert.Empty(result.AppliedPromotions);
    }

    [Fact]
    public async Task CategoryBasedPromotion_AppliesOnlyToMatchingCatalog()
    {
        var now = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);
        var promotion = Promotion(PromotionType.CategoryBased, 10m, now.AddDays(-1), now.AddDays(1), AccessoriesCatalogId);

        var result = await CalculateAsync(
            CustomerLoyaltyTier.Bronze,
            [Item(1000m, 1, BikesCatalogId), Item(200m, 2, AccessoriesCatalogId)],
            [promotion],
            now);

        Assert.Equal(40m, result.PromotionDiscountAmount);
    }

    [Fact]
    public async Task CategoryBasedPromotion_DoesNotApplyToDifferentCatalog()
    {
        var now = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);
        var promotion = Promotion(PromotionType.CategoryBased, 10m, now.AddDays(-1), now.AddDays(1), AccessoriesCatalogId);

        var result = await CalculateAsync(CustomerLoyaltyTier.Bronze, [Item(1000m, 1, BikesCatalogId)], [promotion], now);

        Assert.Equal(0m, result.PromotionDiscountAmount);
        Assert.Empty(result.AppliedPromotions);
    }

    [Fact]
    public async Task FinalAmount_NeverBecomesNegative()
    {
        var now = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);
        var promotion = Promotion(PromotionType.TimeBased, 200m, now.AddDays(-1), now.AddDays(1));

        var result = await CalculateAsync(CustomerLoyaltyTier.Gold, [Item(100m, 1, BikesCatalogId)], [promotion], now);

        Assert.Equal(0m, result.FinalAmount);
    }

    private Task<PriceCalculationResultDto> CalculateAsync(
        CustomerLoyaltyTier tier,
        IReadOnlyCollection<OrderPricingItem> items,
        IReadOnlyCollection<Promotion>? promotions = null,
        DateTime? now = null) =>
        _pricingService.CalculateAsync(
            new Customer { LoyaltyTier = tier },
            items,
            promotions ?? [],
            now ?? new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc));

    private static OrderPricingItem Item(decimal unitPrice, int quantity, Guid catalogId) =>
        new()
        {
            ProductId = Guid.NewGuid(),
            CatalogId = catalogId,
            UnitPrice = unitPrice,
            Quantity = quantity
        };

    private static Promotion Promotion(
        PromotionType type,
        decimal discountPercent,
        DateTime activeFrom,
        DateTime activeTo,
        Guid? catalogId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = $"{type} Promo",
            Description = "Test promotion",
            Type = type,
            DiscountPercent = discountPercent,
            ActiveFrom = activeFrom,
            ActiveTo = activeTo,
            CatalogId = catalogId,
            IsActive = true
        };
}
