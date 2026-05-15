using BicycleShop.Application.DTOs.Orders;
using BicycleShop.Application.DTOs.Pricing;
using BicycleShop.Application.Exceptions;
using BicycleShop.Data.Context;
using BicycleShop.Domain.Entities;
using BicycleShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Application.Services;

public class OrderService : IOrderService
{
    private readonly BicycleShopDbContext _context;
    private readonly IPricingService _pricingService;
    private readonly IOrderWorkflowService _workflowService;
    private readonly IInventoryService _inventoryService;
    private readonly IPromotionService _promotionService;

    public OrderService(
        BicycleShopDbContext context,
        IPricingService pricingService,
        IOrderWorkflowService workflowService,
        IInventoryService inventoryService,
        IPromotionService promotionService)
    {
        _context = context;
        _pricingService = pricingService;
        _workflowService = workflowService;
        _inventoryService = inventoryService;
        _promotionService = promotionService;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request)
    {
        if (request.Items.Count == 0)
        {
            throw new BusinessRuleValidationException("Order must contain at least one item.");
        }

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId)
            ?? throw new NotFoundException($"Customer with id '{request.CustomerId}' was not found.");

        var requestedItems = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new CreateOrderItemRequestDto
            {
                ProductId = g.Key,
                Quantity = g.Sum(i => i.Quantity)
            })
            .ToList();

        var productIds = requestedItems.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Include(p => p.Catalog)
            .Include(p => p.Inventory)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        EnsureAllProductsExist(productIds, products);
        EnsureInventoryAvailable(requestedItems, products);

        var now = DateTime.UtcNow;
        var promotions = await _promotionService.GetActiveEntitiesAsync(now);
        var pricingItems = requestedItems
            .Select(item =>
            {
                var product = products.Single(p => p.Id == item.ProductId);
                return new OrderPricingItem
                {
                    ProductId = product.Id,
                    CatalogId = product.CatalogId,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                };
            })
            .ToList();

        var pricing = await _pricingService.CalculateAsync(customer, pricingItems, promotions, now);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                CustomerId = customer.Id,
                OrderDate = now,
                Status = OrderStatus.Pending,
                Subtotal = pricing.Subtotal,
                LoyaltyDiscountAmount = pricing.LoyaltyDiscountAmount,
                BulkDiscountAmount = pricing.BulkDiscountAmount,
                PromotionDiscountAmount = pricing.PromotionDiscountAmount,
                TotalDiscountAmount = pricing.TotalDiscountAmount,
                TotalAmount = pricing.FinalAmount
            };

            await _context.Orders.AddAsync(order);

            var orderItems = BuildOrderItems(order.Id, requestedItems, products, pricing);
            await _context.OrderItems.AddRangeAsync(orderItems);

            var appliedPromotions = pricing.AppliedPromotions.Select(p => new AppliedPromotion
            {
                OrderId = order.Id,
                PromotionId = p.PromotionId,
                Name = p.Name,
                Description = p.Description,
                DiscountAmount = p.DiscountAmount
            });
            await _context.AppliedPromotions.AddRangeAsync(appliedPromotions);

            foreach (var item in requestedItems)
            {
                await _inventoryService.ReserveAsync(order.Id, item.ProductId, item.Quantity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetOrderByIdAsync(order.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(Guid orderId)
    {
        var order = await GetOrderQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException($"Order with id '{orderId}' was not found.");

        return MapToDto(order);
    }

    public async Task<OrderResponseDto> ChangeStatusAsync(Guid orderId, ChangeOrderStatusRequestDto request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new NotFoundException($"Order with id '{orderId}' was not found.");

            _workflowService.ValidateStatusTransition(order.Status, request.NewStatus);

            if (request.NewStatus == OrderStatus.Cancelled)
            {
                await _inventoryService.ReleaseReservationAsync(order.Id);
            }
            else if (request.NewStatus == OrderStatus.Shipped)
            {
                await _inventoryService.CommitReservationAsync(order.Id);
            }

            order.Status = request.NewStatus;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetOrderByIdAsync(order.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<OrderResponseDto> CancelAsync(Guid orderId) =>
        ChangeStatusAsync(orderId, new ChangeOrderStatusRequestDto { NewStatus = OrderStatus.Cancelled });

    private static void EnsureAllProductsExist(IReadOnlyCollection<Guid> productIds, IReadOnlyCollection<Product> products)
    {
        var foundProductIds = products.Select(p => p.Id).ToHashSet();
        var missingProductIds = productIds.Where(id => !foundProductIds.Contains(id)).ToList();
        if (missingProductIds.Count > 0)
        {
            throw new NotFoundException($"Products were not found: {string.Join(", ", missingProductIds)}.");
        }
    }

    private static void EnsureInventoryAvailable(
        IReadOnlyCollection<CreateOrderItemRequestDto> requestedItems,
        IReadOnlyCollection<Product> products)
    {
        foreach (var item in requestedItems)
        {
            var product = products.Single(p => p.Id == item.ProductId);
            if (product.Inventory is null)
            {
                throw new NotFoundException($"Inventory for product '{product.Name}' was not found.");
            }

            if (product.Inventory.AvailableForReservation < item.Quantity)
            {
                throw new InsufficientInventoryException(
                    $"Insufficient inventory for product '{product.Name}'. Requested {item.Quantity}, available {product.Inventory.AvailableForReservation}.");
            }
        }
    }

    private static List<OrderItem> BuildOrderItems(
        Guid orderId,
        IReadOnlyCollection<CreateOrderItemRequestDto> requestedItems,
        IReadOnlyCollection<Product> products,
        PriceCalculationResultDto pricing)
    {
        var lineSubtotals = requestedItems
            .Select(item =>
            {
                var product = products.Single(p => p.Id == item.ProductId);
                return new
                {
                    Item = item,
                    Product = product,
                    LineSubtotal = RoundMoney(product.Price * item.Quantity)
                };
            })
            .ToList();

        var discountPool = Math.Min(pricing.Subtotal, pricing.TotalDiscountAmount);
        var allocatedDiscount = 0m;
        var orderItems = new List<OrderItem>();

        for (var index = 0; index < lineSubtotals.Count; index++)
        {
            var line = lineSubtotals[index];
            var isLast = index == lineSubtotals.Count - 1;
            var lineDiscount = pricing.Subtotal == 0m
                ? 0m
                : isLast
                    ? RoundMoney(discountPool - allocatedDiscount)
                    : RoundMoney(discountPool * (line.LineSubtotal / pricing.Subtotal));

            allocatedDiscount += lineDiscount;

            orderItems.Add(new OrderItem
            {
                OrderId = orderId,
                ProductId = line.Product.Id,
                Quantity = line.Item.Quantity,
                UnitPrice = line.Product.Price,
                LineSubtotal = line.LineSubtotal,
                DiscountAmount = lineDiscount,
                LineTotal = Math.Max(0m, RoundMoney(line.LineSubtotal - lineDiscount))
            });
        }

        return orderItems;
    }

    private IQueryable<Order> GetOrderQuery() =>
        _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.AppliedPromotions)
            .Include(o => o.Reservations)
            .ThenInclude(r => r.Product);

    private static OrderResponseDto MapToDto(Order order) =>
        new()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}".Trim(),
            CustomerEmail = order.Customer.Email,
            Status = order.Status,
            OrderDate = order.OrderDate,
            Subtotal = order.Subtotal,
            LoyaltyDiscountAmount = order.LoyaltyDiscountAmount,
            BulkDiscountAmount = order.BulkDiscountAmount,
            PromotionDiscountAmount = order.PromotionDiscountAmount,
            TotalDiscountAmount = order.TotalDiscountAmount,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineSubtotal = i.LineSubtotal,
                LineTotal = i.LineTotal
            }).ToList(),
            AppliedPromotions = order.AppliedPromotions.Select(p => new AppliedPromotionDto
            {
                PromotionId = p.PromotionId,
                Name = p.Name,
                Description = p.Description,
                DiscountAmount = p.DiscountAmount
            }).ToList(),
            Reservations = order.Reservations.Select(r => new OrderReservationResponseDto
            {
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                Quantity = r.Quantity,
                IsReleased = r.IsReleased,
                IsCommitted = r.IsCommitted,
                CreatedAt = r.CreatedAt,
                ReleasedAt = r.ReleasedAt
            }).ToList()
        };

    private static decimal RoundMoney(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
