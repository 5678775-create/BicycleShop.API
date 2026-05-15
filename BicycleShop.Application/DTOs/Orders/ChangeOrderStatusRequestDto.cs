using BicycleShop.Domain.Enums;

namespace BicycleShop.Application.DTOs.Orders;

public class ChangeOrderStatusRequestDto
{
    public OrderStatus NewStatus { get; set; }
}
