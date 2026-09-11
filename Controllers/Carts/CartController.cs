using Asp.Versioning;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Carts;

[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/cart")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
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

            throw new UnauthorizedAccessException("User ID claim is missing or invalid in token.");
        }
    }

    // GET: api/cart
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart(CancellationToken cancellationToken = default)
    {
        var cart = await _cartService.GetCartByCustomerIdAsync(CurrentUserId, cancellationToken);

        return Ok(new ApiResponse<CartDto>
        {
            Success = true,
            Message = "Cart retrieved successfully.",
            Data = cart
        });
    }

    // GET: api/cart/count 
    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<int>>> GetCartItemCount(CancellationToken cancellationToken = default)
    {
        var count = await _cartService.GetCartItemCountAsync(CurrentUserId, cancellationToken);

        return Ok(new ApiResponse<int>
        {
            Success = true,
            Message = "Cart item count retrieved successfully.",
            Data = count
        });
    }

    // GET: api/cart/subtotal 
    [HttpGet("subtotal")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetCartSubtotal(CancellationToken cancellationToken = default)
    {
        var subtotal = await _cartService.GetCartSubtotalAsync(CurrentUserId, cancellationToken);

        return Ok(new ApiResponse<decimal>
        {
            Success = true,
            Message = "Cart subtotal retrieved successfully.",
            Data = subtotal
        });
    }

    // POST: api/cart/items (Add Item to Cart)
    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddItem([FromBody] AddToCartDto dto, CancellationToken cancellationToken = default)
    {
        var updatedCart = await _cartService.AddItemToCartAsync(CurrentUserId, dto, cancellationToken);

        return Ok(new ApiResponse<CartDto>
        {
            Success = true,
            Message = "Item added to cart successfully.",
            Data = updatedCart
        });
    }

    // PUT: api/cart/items/{productId} (Update Quantity)
    [HttpPut("items/{productId:int}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateQuantity(int productId, [FromBody] UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return BadRequest(new ApiResponse<CartDto>
            {
                Success = false,
                Message = "Product ID must be greater than 0."
            });
        }

        var updatedCart = await _cartService.UpdateCartItemQuantityAsync(CurrentUserId, productId, dto, cancellationToken);

        return Ok(new ApiResponse<CartDto>
        {
            Success = true,
            Message = "Cart item quantity updated successfully.",
            Data = updatedCart
        });
    }

    // DELETE: api/cart/items/{productId} (Remove Single Item)
    [HttpDelete("items/{productId:int}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> RemoveItem(int productId, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return BadRequest(new ApiResponse<CartDto>
            {
                Success = false,
                Message = "Product ID must be greater than 0."
            });
        }

        var updatedCart = await _cartService.RemoveCartItemAsync(CurrentUserId, productId, cancellationToken);

        return Ok(new ApiResponse<CartDto>
        {
            Success = true,
            Message = "Item removed from cart successfully.",
            Data = updatedCart
        });
    }

    // DELETE: api/cart (Clear Entire Cart)
    [HttpDelete]
    public async Task<ActionResult<ApiResponse<CartDto>>> ClearCart(CancellationToken cancellationToken = default)
    {
        var updatedCart = await _cartService.ClearCartAsync(CurrentUserId, cancellationToken);

        return Ok(new ApiResponse<CartDto>
        {
            Success = true,
            Message = "Cart cleared successfully.",
            Data = updatedCart
        });
    }
}
