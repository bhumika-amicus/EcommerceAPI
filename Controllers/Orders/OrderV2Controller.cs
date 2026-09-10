using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.DTOs.Orders;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Orders;

[Authorize]
[ApiVersion(2.0)]
[ApiController]
[Route("api/v{version:apiVersion}/orders")]
public class OrderV2Controller : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderV2Controller(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private int CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? User.FindFirst("sub")?.Value;

            if (int.TryParse(claim, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException(
                "User ID claim is missing or invalid in token.");
        }
    }

    // POST: api/v2/orders
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CheckoutRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderService.CreateOrderAsync(
            CurrentUserId,
            request,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Order created successfully.",
            data = order
        });
    }

    // GET: api/v2/orders/{orderId}
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrder(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
        {
            return BadRequest(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Order ID must be greater than 0."
            });
        }

        var order = await _orderService.GetOrderByIdAsync(
            CurrentUserId,
            orderId,
            cancellationToken);

        if (order == null)
        {
            return NotFound(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Order not found."
            });
        }

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Order retrieved successfully.",
            data = order
        });
    }

    // GET: api/v2/orders
    [HttpGet]
    public async Task<IActionResult> GetCustomerOrders(
        [FromQuery] OrderQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetCustomerOrdersPagedAsync(
            CurrentUserId,
            query,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Customer orders retrieved successfully.",
            data = result
        });
    }

    // POST: api/v2/orders/{orderId}/reorder
    [HttpPost("{orderId:int}/reorder")]
    public async Task<IActionResult> Reorder(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
        {
            return BadRequest(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Order ID must be greater than 0."
            });
        }

        var cart = await _orderService.ReorderAsync(
            CurrentUserId,
            orderId,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = $"Items from order {orderId} added to cart successfully.",
            data = cart
        });
    }

    // PUT: api/v2/orders/{orderId}/cancel
    [HttpPut("{orderId:int}/cancel")]
    public async Task<IActionResult> CancelOrder(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
        {
            return BadRequest(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Order ID must be greater than 0."
            });
        }

        var cancelledOrder = await _orderService.CancelOrderAsync(
            CurrentUserId,
            orderId,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = $"Order {orderId} has been cancelled successfully and product stock was restored.",
            data = cancelledOrder
        });
    }
}