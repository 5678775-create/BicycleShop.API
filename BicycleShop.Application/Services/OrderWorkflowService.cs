using BicycleShop.Application.Exceptions;
using BicycleShop.Domain.Enums;

namespace BicycleShop.Application.Services;

public class OrderWorkflowService : IOrderWorkflowService
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = []
    };

    public bool CanChangeStatus(OrderStatus currentStatus, OrderStatus newStatus) =>
        AllowedTransitions.TryGetValue(currentStatus, out var allowedStatuses) &&
        allowedStatuses.Contains(newStatus);

    public void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (CanChangeStatus(currentStatus, newStatus))
        {
            return;
        }

        throw new InvalidOrderStatusTransitionException(
            $"Order status transition from {currentStatus} to {newStatus} is not allowed.");
    }
}
