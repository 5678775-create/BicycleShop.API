using BicycleShop.Application.Exceptions;
using BicycleShop.Application.Services;
using BicycleShop.Domain.Enums;

namespace BicycleShop.Tests;

public class OrderWorkflowServiceTests
{
    private readonly OrderWorkflowService _workflowService = new();

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
    public void CanChangeStatus_ReturnsTrue_ForAllowedTransitions(OrderStatus currentStatus, OrderStatus newStatus)
    {
        Assert.True(_workflowService.CanChangeStatus(currentStatus, newStatus));
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed)]
    public void CanChangeStatus_ReturnsFalse_ForForbiddenTransitions(OrderStatus currentStatus, OrderStatus newStatus)
    {
        Assert.False(_workflowService.CanChangeStatus(currentStatus, newStatus));
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed)]
    public void ValidateStatusTransition_Throws_ForForbiddenTransitions(OrderStatus currentStatus, OrderStatus newStatus)
    {
        var exception = Assert.Throws<InvalidOrderStatusTransitionException>(
            () => _workflowService.ValidateStatusTransition(currentStatus, newStatus));

        Assert.Equal(
            $"Order status transition from {currentStatus} to {newStatus} is not allowed.",
            exception.Message);
    }
}
