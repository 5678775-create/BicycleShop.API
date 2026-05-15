using BicycleShop.Domain.Enums;

namespace BicycleShop.Application.Services;

public interface IOrderWorkflowService
{
    bool CanChangeStatus(OrderStatus currentStatus, OrderStatus newStatus);
    void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus);
}
