using BicycleShop.Application.DTOs.Inventory;
using BicycleShop.Application.Exceptions;
using BicycleShop.Data.Context;
using BicycleShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly BicycleShopDbContext _context;

    public InventoryService(BicycleShopDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryResponseDto>> GetAllAsync()
    {
        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .OrderBy(i => i.Product.Name)
            .ToListAsync();

        return inventories.Select(MapToDto);
    }

    public async Task<bool> CheckAvailabilityAsync(Guid productId, int quantity)
    {
        var inventory = await GetInventoryEntityAsync(productId);
        return inventory.AvailableForReservation >= quantity;
    }

    public async Task<InventoryReservation> ReserveAsync(Guid orderId, Guid productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleValidationException("Reservation quantity must be greater than 0.");
        }

        var inventory = await GetInventoryEntityAsync(productId);
        if (inventory.AvailableForReservation < quantity)
        {
            throw new InsufficientInventoryException(
                $"Insufficient inventory for product '{inventory.Product.Name}'. Requested {quantity}, available {inventory.AvailableForReservation}.");
        }

        inventory.QuantityReserved += quantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        var reservation = new InventoryReservation
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        };

        await _context.InventoryReservations.AddAsync(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    public async Task ReleaseReservationAsync(Guid orderId)
    {
        var reservations = await GetActiveReservations(orderId).ToListAsync();
        foreach (var reservation in reservations)
        {
            var inventory = await GetInventoryEntityAsync(reservation.ProductId);
            if (inventory.QuantityReserved < reservation.Quantity)
            {
                throw new BusinessRuleValidationException(
                    $"Inventory reservation for product '{inventory.Product.Name}' cannot be released because reserved quantity is inconsistent.");
            }

            inventory.QuantityReserved -= reservation.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            reservation.IsReleased = true;
            reservation.ReleasedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task CommitReservationAsync(Guid orderId)
    {
        var reservations = await GetActiveReservations(orderId).ToListAsync();
        foreach (var reservation in reservations)
        {
            var inventory = await GetInventoryEntityAsync(reservation.ProductId);
            if (inventory.QuantityReserved < reservation.Quantity || inventory.QuantityAvailable < reservation.Quantity)
            {
                throw new BusinessRuleValidationException(
                    $"Inventory reservation for product '{inventory.Product.Name}' cannot be committed because stock is inconsistent.");
            }

            inventory.QuantityAvailable -= reservation.Quantity;
            inventory.QuantityReserved -= reservation.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            inventory.Product.StockQuantity = inventory.QuantityAvailable;

            reservation.IsCommitted = true;
            reservation.IsReleased = true;
            reservation.ReleasedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<InventoryResponseDto> GetInventoryByProductIdAsync(Guid productId)
    {
        var inventory = await GetInventoryEntityAsync(productId);
        return MapToDto(inventory);
    }

    public async Task<InventoryResponseDto> UpdateInventoryAsync(Guid productId, int quantityAvailable)
    {
        if (quantityAvailable < 0)
        {
            throw new BusinessRuleValidationException("QuantityAvailable cannot be negative.");
        }

        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory is null)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new NotFoundException($"Product with id '{productId}' was not found.");

            inventory = new Inventory
            {
                ProductId = productId,
                Product = product,
                QuantityAvailable = quantityAvailable,
                QuantityReserved = 0,
                UpdatedAt = DateTime.UtcNow
            };

            product.StockQuantity = quantityAvailable;
            await _context.Inventories.AddAsync(inventory);
        }
        else
        {
            if (quantityAvailable < inventory.QuantityReserved)
            {
                throw new BusinessRuleValidationException(
                    "QuantityAvailable cannot be lower than current reserved quantity.");
            }

            inventory.QuantityAvailable = quantityAvailable;
            inventory.UpdatedAt = DateTime.UtcNow;
            inventory.Product.StockQuantity = quantityAvailable;
        }

        await _context.SaveChangesAsync();
        return MapToDto(inventory);
    }

    private IQueryable<InventoryReservation> GetActiveReservations(Guid orderId) =>
        _context.InventoryReservations
            .Where(r => r.OrderId == orderId && !r.IsReleased && !r.IsCommitted);

    private async Task<Inventory> GetInventoryEntityAsync(Guid productId) =>
        await _context.Inventories
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId)
        ?? throw new NotFoundException($"Inventory for product id '{productId}' was not found.");

    private static InventoryResponseDto MapToDto(Inventory inventory) =>
        new()
        {
            ProductId = inventory.ProductId,
            ProductName = inventory.Product.Name,
            QuantityAvailable = inventory.QuantityAvailable,
            QuantityReserved = inventory.QuantityReserved,
            QuantityAvailableForReservation = inventory.AvailableForReservation,
            UpdatedAt = inventory.UpdatedAt
        };
}
