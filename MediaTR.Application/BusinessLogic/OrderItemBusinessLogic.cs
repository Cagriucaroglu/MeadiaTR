using MediaTR.Domain.Entities;
using MediaTR.Domain.Repositories;

namespace MediaTR.Application.BusinessLogic;

public class OrderItemBusinessLogic
{
    private readonly IProductRepository _productRepository;

    public OrderItemBusinessLogic(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<OrderItem> CreateOrderItem(Guid orderId, Guid productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new InvalidOperationException($"Product with Id {productId} not found");

        if (!product.IsInStock || product.StockQuantity < quantity)
            throw new InvalidOperationException($"Insufficient stock for product {product.Name}");

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = product.Price
        };

        return orderItem;
    }

    public void ValidateOrderItem(OrderItem orderItem)
    {
        if (orderItem.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(orderItem.Quantity));

        if (orderItem.UnitPrice.Amount <= 0)
            throw new ArgumentException("Unit price must be greater than zero", nameof(orderItem.UnitPrice));
    }

}