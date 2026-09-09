using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Checkout;

[Authorize]
[ApiController]
[Route("api/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly IShippingMethodService _shippingMethodService;

    public CheckoutController(ICheckoutService checkoutService, IShippingMethodService shippingMethodService)
    {
        _checkoutService = checkoutService;
        _shippingMethodService = shippingMethodService;
    }

    private int CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value?? User.FindFirst("sub")?.Value;

            if (int.TryParse(claim, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException( "User ID claim is missing or invalid in token.");
        }
    }

    // GET: api/checkout/shipping-methods OR api/shipping-methods
    [HttpGet("shipping-methods")]
    [HttpGet("/api/shipping-methods")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ShippingMethodDto>>>> GetShippingMethods(CancellationToken cancellationToken = default)
    {
        var methods = await _shippingMethodService.GetAllShippingMethodsAsync(cancellationToken);

        return Ok(new ApiResponse<IEnumerable<ShippingMethodDto>>
        {
            Success = true,
            Message = "Shipping methods retrieved successfully.",
            Data = methods
        });
    }

    // POST: api/checkout
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CheckoutDto>>> PreviewCheckout( [FromBody] CheckoutRequestDto dto, CancellationToken cancellationToken = default)
    {
        var checkout = await _checkoutService.PreviewCheckoutAsync( CurrentUserId, dto, cancellationToken);

        return Ok(new ApiResponse<CheckoutDto>
        {
            Success = true,
            Message = "Checkout details calculated successfully.",
            Data = checkout
        });
    }
}