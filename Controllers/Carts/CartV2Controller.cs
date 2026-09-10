using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Carts;

[Authorize]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/cart")]
[ApiController]
public class CartV2Controller : ControllerBase
{
    private readonly ICartService _cartService;

    public CartV2Controller(ICartService cartService)
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

            throw new UnauthorizedAccessException(
                "User ID claim is missing or invalid in token.");
        }
    }

    // GET: api/v2/cart
    [HttpGet]
    public async Task<IActionResult> GetCart(
        CancellationToken cancellationToken = default)
    {
        var cart = await _cartService.GetCartByCustomerIdAsync(
            CurrentUserId,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Cart retrieved successfully.",
            data = cart
        });
    }

    // GET: api/v2/cart/count
    [HttpGet("count")]
    public async Task<IActionResult> GetCartItemCount(
        CancellationToken cancellationToken = default)
    {
        var count = await _cartService.GetCartItemCountAsync(
            CurrentUserId,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Cart item count retrieved successfully.",
            data = count
        });
    }

    // GET: api/v2/cart/subtotal
    [HttpGet("subtotal")]
    public async Task<IActionResult> GetCartSubtotal(
        CancellationToken cancellationToken = default)
    {
        var subtotal = await _cartService.GetCartSubtotalAsync(
            CurrentUserId,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Cart subtotal retrieved successfully.",
            data = subtotal
        });
    }

    // POST: api/v2/cart/items
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddToCartDto dto,
        CancellationToken cancellationToken = default)
    {
        var updatedCart = await _cartService.AddItemToCartAsync(
            CurrentUserId,
            dto,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Item added to cart successfully.",
            data = updatedCart
        });
    }

    // PUT: api/v2/cart/items/{productId}
    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateQuantity(
        int productId,
        [FromBody] UpdateCartItemDto dto,
        CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return BadRequest(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Product ID must be greater than 0."
            });
        }

        var updatedCart = await _cartService.UpdateCartItemQuantityAsync(
            CurrentUserId,
            productId,
            dto,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Cart item quantity updated successfully.",
            data = updatedCart
        });
    }

    // DELETE: api/v2/cart/items/{productId}
    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(
        int productId,
        CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return BadRequest(new
            {
                apiVersion = "2.0",
                success = false,
                message = "Product ID must be greater than 0."
            });
        }

        var updatedCart = await _cartService.RemoveCartItemAsync(
            CurrentUserId,
            productId,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Item removed from cart successfully.",
            data = updatedCart
        });
    }

    // DELETE: api/v2/cart
    [HttpDelete]
    public async Task<IActionResult> ClearCart(
        CancellationToken cancellationToken = default)
    {
        var updatedCart = await _cartService.ClearCartAsync(
            CurrentUserId,
            cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Cart cleared successfully.",
            data = updatedCart
        });
    }
}