namespace BicycleShop.Application.DTOs.Orders;

public class CreateOrderRequestDto
{
    public Guid CustomerId { get; set; }
    public List<CreateOrderItemRequestDto> Items { get; set; } = new();
}
