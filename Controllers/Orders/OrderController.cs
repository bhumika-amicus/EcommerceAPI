using Asp.Versioning;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EcommerceAPI.Common;
using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.DTOs.Orders;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Orders;

[Authorize]
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
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

            throw new UnauthorizedAccessException( "User ID claim is missing or invalid in token.");
        }
    }

    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder( [FromBody] CheckoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderService.CreateOrderAsync( CurrentUserId, request, cancellationToken);

        return Ok(new ApiResponse<OrderDto>
        {
            Success = true,
            Message = "Order created successfully.",
            Data = order
        });
    }

    // GET: api/orders/{orderId}
    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder( int orderId, CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
        {
            return BadRequest(new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Order ID must be greater than 0."
            });
        }

        var order = await _orderService.GetOrderByIdAsync( CurrentUserId, orderId, cancellationToken);

        if (order == null)
        {
            return NotFound(new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Order not found."
            });
        }

        return Ok(new ApiResponse<OrderDto>
        {
            Success = true,
            Message = "Order retrieved successfully.",
            Data = order
        });
    }

    // GET: api/orders
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetCustomerOrders( [FromQuery] OrderQueryDto query, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetCustomerOrdersPagedAsync(CurrentUserId, query, cancellationToken);

        return Ok(new ApiResponse<PagedResult<OrderDto>>
        {
            Success = true,
            Message = "Customer orders retrieved successfully.",
            Data = result
        });
    }

    // POST: api/orders/{orderId}/reorder
    [HttpPost("{orderId:int}/reorder")]
    public async Task<ActionResult<ApiResponse<CartDto>>> Reorder(int orderId, CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
        {
            return BadRequest(new ApiResponse<CartDto>
            {
                Success = false,
                Message = "Order ID must be greater than 0."
            });
        }

        var cart = await _orderService.ReorderAsync(CurrentUserId, orderId, cancellationToken);

        return Ok(new ApiResponse<CartDto>
        {
            Success = true,
            Message = $"Items from order {orderId} added to cart successfully.",
            Data = cart
        });
    }

    // PUT: api/orders/{orderId}/cancel
    [HttpPut("{orderId:int}/cancel")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CancelOrder(int orderId, CancellationToken cancellationToken = default)
    {
        if (orderId <= 0)
        {
            return BadRequest(new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Order ID must be greater than 0."
            });
        }

        var cancelledOrder = await _orderService.CancelOrderAsync(CurrentUserId, orderId, cancellationToken);

        return Ok(new ApiResponse<OrderDto>
        {
            Success = true,
            Message = $"Order {orderId} has been cancelled successfully and product stock was restored.",
            Data = cancelledOrder
        });
    }
}

