using BicycleShop.Application.DTOs.Promotions;
using BicycleShop.Application.Exceptions;
using BicycleShop.Data.Context;
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Application.Services;

public class PromotionService : IPromotionService
{
    private readonly BicycleShopDbContext _context;

    public PromotionService(BicycleShopDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PromotionResponseDto>> GetAllAsync()
    {
        var promotions = await _context.Promotions
            .OrderBy(p => p.Name)
            .ToListAsync();

        var now = DateTime.UtcNow;
        return promotions.Select(p => MapToDto(p, now));
    }

    public async Task<IEnumerable<PromotionResponseDto>> GetActiveAsync(DateTime now)
    {
        var promotions = await GetActiveQuery(now)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return promotions.Select(p => MapToDto(p, now));
    }

    public async Task<IReadOnlyCollection<Promotion>> GetActiveEntitiesAsync(DateTime now) =>
        await GetActiveQuery(now).ToListAsync();

    public async Task<PromotionResponseDto> CreateAsync(CreatePromotionRequestDto request)
    {
        if (request.Type == PromotionType.CategoryBased && !request.CatalogId.HasValue)
        {
            throw new BusinessRuleValidationException("CatalogId is required for category based promotions.");
        }

        if (request.CatalogId.HasValue)
        {
            var catalogExists = await _context.Catalogs.AnyAsync(c => c.Id == request.CatalogId.Value);
            if (!catalogExists)
            {
                throw new NotFoundException($"Catalog with id '{request.CatalogId.Value}' was not found.");
            }
        }

        var promotion = new Promotion
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            DiscountPercent = request.DiscountPercent,
            ActiveFrom = request.ActiveFrom,
            ActiveTo = request.ActiveTo,
            CatalogId = request.CatalogId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Promotions.AddAsync(promotion);
        await _context.SaveChangesAsync();

        return MapToDto(promotion, DateTime.UtcNow);
    }

    private IQueryable<Promotion> GetActiveQuery(DateTime now) =>
        _context.Promotions
            .Where(p => p.IsActive && p.ActiveFrom <= now && p.ActiveTo >= now);

    private static PromotionResponseDto MapToDto(Promotion promotion, DateTime now) =>
        new()
        {
            Id = promotion.Id,
            Name = promotion.Name,
            Description = promotion.Description,
            Type = promotion.Type,
            DiscountPercent = promotion.DiscountPercent,
            ActiveFrom = promotion.ActiveFrom,
            ActiveTo = promotion.ActiveTo,
            CatalogId = promotion.CatalogId,
            IsCurrentlyActive = promotion.IsActive && promotion.ActiveFrom <= now && promotion.ActiveTo >= now
        };
}
