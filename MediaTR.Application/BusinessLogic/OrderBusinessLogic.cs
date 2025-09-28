using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Events;
using MediaTR.Domain.Repositories;

namespace MediaTR.Application.BusinessLogic;

public class OrderBusinessLogic
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderBusinessLogic(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task PlaceOrder(Order order, Guid correlationId)
    {
        // Business rule validation
        if (order.OrderItems == null || !order.OrderItems.Any())
            throw new InvalidOperationException("Order must contain at least one item");

        if (order.TotalAmount.Amount <= 0)
            throw new InvalidOperationException("Order total amount must be greater than zero");

        // Generate unique order number
        order.OrderNumber = await _orderRepository.GenerateOrderNumberAsync();

        // Validate stock availability
        foreach (var item in order.OrderItems)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product {item.ProductId} not found");

            if (!product.IsInStock || product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
        }

        // Set order status and dates
        order.Status = OrderStatus.Pending;
        order.OrderDate = DateTime.UtcNow;
        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Raise domain event with request CorrelationId
        order.Raise(new OrderPlacedEvent
        {
            Payload = order,
            CorrelationId = correlationId
        });
    }

    public void ConfirmOrder(Order order)
    {
        // Business rule validation
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        order.Status = OrderStatus.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;
    }

    public void ProcessOrder(Order order)
    {
        // Business rule validation
        if (order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be processed");

        order.Status = OrderStatus.Processing;
        order.UpdatedAt = DateTime.UtcNow;
    }

    public void ShipOrder(Order order, string trackingNumber)
    {
        // Business rule validation
        if (order.Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped");

        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required for shipping");

        order.Status = OrderStatus.Shipped;
        order.ShippedDate = DateTime.UtcNow;
        order.TrackingNumber = trackingNumber;
        order.UpdatedAt = DateTime.UtcNow;
    }

    public void DeliverOrder(Order order)
    {
        // Business rule validation
        if (order.Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered");

        order.Status = OrderStatus.Delivered;
        order.DeliveredDate = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
    }

    public void CancelOrder(Order order)
    {
        // Business rule validation
        if (!order.CanBeCancelled)
            throw new InvalidOperationException("Order cannot be cancelled in current status");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
    }

    public void AddOrderItem(Order order, OrderItem orderItem)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (orderItem == null)
            throw new ArgumentNullException(nameof(orderItem));

        order.OrderItems.Add(orderItem);
    }

    public void ClearOrderItems(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        order.OrderItems.Clear();
    }

}