using MediatR;
using MediaTR.Application.BusinessLogic;
using MediaTR.Domain.Entities;
using MediaTR.Domain.Entities.InMemory;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.Services;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MediaTR.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// CreateOrder command handler
/// Checkout flow: Cart → Order → Payment → Stock Deduction
/// </summary>
internal sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResult>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly OrderBusinessLogic _orderBusinessLogic;
    private readonly OrderItemBusinessLogic _orderItemBusinessLogic;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IShoppingCartService shoppingCartService,
        OrderBusinessLogic orderBusinessLogic,
        OrderItemBusinessLogic orderItemBusinessLogic,
        IHttpContextAccessor httpContextAccessor)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _shoppingCartService = shoppingCartService;
        _orderBusinessLogic = orderBusinessLogic;
        _orderItemBusinessLogic = orderItemBusinessLogic;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<CreateOrderResult>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get authenticated user
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return Error.Failure("Http.ContextNotAvailable", "HTTP context is not available");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Error.Unauthorized("Auth.Unauthorized", "User must be authenticated to create order");

        // 2. Get shopping cart
        var cart = await _shoppingCartService.GetCartAsync(userId, cancellationToken);
        if (cart == null || cart.Items.Count == 0)
            return Error.Validation("Order.EmptyCart", "Cannot create order from empty cart");

        // 3. Validate all products and stock
        var validationErrors = await ValidateCartItems(cart, cancellationToken);
        if (validationErrors.Any())
            return Error.Validation("Order.ValidationFailed", string.Join("; ", validationErrors));

        // 4. Create Address ValueObjects
        var shippingAddress = Address.Create(
            request.ShippingStreet,
            request.ShippingCity,
            request.ShippingState,
            request.ShippingPostalCode,
            request.ShippingCountry);

        var billingAddress = !string.IsNullOrWhiteSpace(request.BillingStreet)
            ? Address.Create(
                request.BillingStreet!,
                request.BillingCity!,
                request.BillingState ?? "",
                request.BillingPostalCode ?? "",
                request.BillingCountry!)
            : shippingAddress;

        // 5. Calculate totals
        var currency = cart.Items.First().Currency; // Assume all items same currency
        var subtotal = cart.Items.Sum(item => item.UnitPrice * item.Quantity);
        var shippingCost = CalculateShippingCost(subtotal);
        var taxAmount = CalculateTax(subtotal);
        var totalAmount = subtotal + shippingCost + taxAmount;

        // 6. Create Order entity
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            TotalAmount = Money.FromDecimal(totalAmount, currency),
            ShippingCost = Money.FromDecimal(shippingCost, currency),
            TaxAmount = Money.FromDecimal(taxAmount, currency),
            DiscountAmount = Money.FromDecimal(0, currency),
            OrderItems = new List<OrderItem>()
        };

        // 7. Create OrderItems from Cart
        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
            if (product == null)
                continue; // Already validated above

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                UnitPrice = Money.FromDecimal(cartItem.UnitPrice, cartItem.Currency),
                Quantity = cartItem.Quantity,
                DiscountAmount = Money.FromDecimal(0, cartItem.Currency)
            };

            _orderBusinessLogic.AddOrderItem(order, orderItem);
        }

        // 8. Place order using BusinessLogic (generates order number, validates)
        await _orderBusinessLogic.PlaceOrder(order, request.CorrelationId);

        // 9. Save order to MongoDB
        await _orderRepository.AddAsync(order, request.CorrelationId, cancellationToken: cancellationToken);

        // 10. Deduct stock from products
        await DeductStockAsync(cart, cancellationToken);

        // 11. Clear shopping cart
        await _shoppingCartService.ClearCartAsync(userId, cancellationToken);

        // 12. Return result
        return new CreateOrderResult(
            order.Id,
            order.OrderNumber,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.Status.ToString());
    }

    private async Task<List<string>> ValidateCartItems(ShoppingCart cart, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);

            if (product == null)
            {
                errors.Add($"Product {cartItem.ProductName} not found");
                continue;
            }

            if (!product.IsActive)
            {
                errors.Add($"Product {product.Name} is no longer available");
                continue;
            }

            if (product.StockQuantity < cartItem.Quantity)
            {
                errors.Add($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}, Requested: {cartItem.Quantity}");
            }
        }

        return errors;
    }

    private async Task DeductStockAsync(ShoppingCart cart, CancellationToken cancellationToken)
    {
        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
            if (product == null)
                continue;

            // Deduct stock
            product.StockQuantity -= cartItem.Quantity;

            // Update product in MongoDB
            await _productRepository.UpdateAsync(product, cancellationToken: cancellationToken);
        }
    }

    private static decimal CalculateShippingCost(decimal subtotal)
    {
        // Simple shipping calculation
        // TODO: Implement real shipping cost calculation based on weight, distance, etc.
        if (subtotal >= 100) return 0; // Free shipping over $100
        return 10; // Flat rate $10
    }

    private static decimal CalculateTax(decimal subtotal)
    {
        // Simple tax calculation (10% VAT)
        // TODO: Implement real tax calculation based on location, product type, etc.
        return subtotal * 0.10m;
    }
}
