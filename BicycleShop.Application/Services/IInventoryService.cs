using BicycleShop.Application.DTOs.Inventory;
using BicycleShop.Domain.Entities;

namespace BicycleShop.Application.Services;

public interface IInventoryService
{
    Task<IEnumerable<InventoryResponseDto>> GetAllAsync();
    Task<bool> CheckAvailabilityAsync(Guid productId, int quantity);
    Task<InventoryReservation> ReserveAsync(Guid orderId, Guid productId, int quantity);
    Task ReleaseReservationAsync(Guid orderId);
    Task CommitReservationAsync(Guid orderId);
    Task<InventoryResponseDto> GetInventoryByProductIdAsync(Guid productId);
    Task<InventoryResponseDto> UpdateInventoryAsync(Guid productId, int quantityAvailable);
}
