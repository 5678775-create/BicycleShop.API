using BicycleShop.Application.DTOs.Orders;

namespace BicycleShop.Application.Services;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request);
    Task<OrderResponseDto> GetOrderByIdAsync(Guid orderId);
    Task<OrderResponseDto> ChangeStatusAsync(Guid orderId, ChangeOrderStatusRequestDto request);
    Task<OrderResponseDto> CancelAsync(Guid orderId);
}
