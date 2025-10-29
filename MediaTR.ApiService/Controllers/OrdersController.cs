using MediatR;
using MediaTR.Application.Features.Orders.Commands;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel.ResultAndError;
using Microsoft.AspNetCore.Mvc;

namespace MediaTR.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new order with fire-and-wait outbox pattern
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Create DTO
        PlaceOrderDto dto = new(
            request.UserId,
            request.OrderItems,
            request.ShippingAddress,
            request.BillingAddress,
            request.Notes,
            request.PaymentMethod
        );

        // Create command with CommandWrapper pattern
        PlaceOrderCommand command = new(dto, Guid.NewGuid());

        // Execute command
        Result<Guid> result = await _mediator.Send(command);

        // Return result
        return result.IsSuccess
            ? Ok(new { OrderId = result.Value, Message = "Order created successfully" })
            : BadRequest(new { Error = result.Error?.Description });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        // TODO: Implement GetOrderQuery
        return Ok($"Get order {id}");
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserOrders(Guid userId)
    {
        // TODO: Implement GetUserOrdersQuery
        return Ok($"Get orders for user {userId}");
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        // TODO: Implement GetOrdersQuery
        return Ok("Get all orders");
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(string id)
    {
        // TODO: Implement UpdateOrderStatusCommand
        return Ok($"Update order {id} status");
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingOrders()
    {
        // TODO: Implement GetPendingOrdersQuery
        return Ok("Get pending orders");
    }
}

/// <summary>
/// Create order request DTO for API
/// </summary>
public record CreateOrderRequest(
    Guid UserId,
    List<OrderItemRequest> OrderItems,
    Address ShippingAddress,
    Address BillingAddress,
    string? Notes = null,
    string? PaymentMethod = null);