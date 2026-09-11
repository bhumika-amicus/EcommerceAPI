using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using EcommerceAPI.Common.Attributes;
using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Checkout;

[Authorize]
[ApiVersion(2.0)]
[ApiController]
[Route("api/v{version:apiVersion}/checkout")]
public class CheckoutV2Controller : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly IShippingMethodService _shippingMethodService;

    public CheckoutV2Controller(
        ICheckoutService checkoutService,
        IShippingMethodService shippingMethodService)
    {
        _checkoutService = checkoutService;
        _shippingMethodService = shippingMethodService;
    }

    private int CurrentUserId
    {
        get
        {
            var claim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
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

    // GET: api/v2/checkout/shipping-methods
    [HttpGet("shipping-methods")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetShippingMethods(
        CancellationToken cancellationToken = default)
    {
        var methods = await _shippingMethodService
            .GetAllShippingMethodsAsync(cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Shipping methods retrieved successfully.",
            data = methods
        });
    }

    // POST: api/v2/checkout
    [HttpPost]
    [AuditLog("CHECKOUT_START", "Checkouts")]
    public async Task<IActionResult> PreviewCheckout(
        [FromBody] CheckoutRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var checkout = await _checkoutService
            .PreviewCheckoutAsync(
                CurrentUserId,
                dto,
                cancellationToken);

        return Ok(new
        {
            apiVersion = "2.0",
            success = true,
            message = "Checkout details calculated successfully.",
            data = checkout
        });
    }
}