using Asp.Versioning;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Payments;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EcommerceAPI.Controllers.Payments;

[Authorize]
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/orders/{orderId}/payments")] 
public class PaymentController : ControllerBase {

    private readonly IPaymentService _paymentService;
    public PaymentController(IPaymentService paymentService) { 
        _paymentService = paymentService; 
    } 
    private int CurrentUserId {
        get { 
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst("sub")?.Value;
            if (int.TryParse(claim, out var userId)) {
                return userId; 
            }
            throw new UnauthorizedAccessException("User ID claim is missing or invalid in token.");
        } 
    } 
    
    // POST: api/orders/{orderId}/payments
    [HttpPost] public async Task<ActionResult<ApiResponse<PaymentDto>>> ProcessPayment( int orderId, [FromBody] PaymentRequestDto request, CancellationToken cancellationToken = default) {
        
        var payment = await _paymentService.ProcessPaymentAsync( CurrentUserId, orderId, request, cancellationToken);
        
        return Ok(new ApiResponse<PaymentDto> { 
            Success = true,
            Message = payment.PaymentStatus == "Successful" ?"Payment processed successfully." : "Payment failed.",
            Data = payment });
    } 

    // GET: api/orders/{orderId}/payments
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetPaymentsForOrder(int orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentService.GetPaymentsByOrderIdAsync(CurrentUserId, orderId, cancellationToken);

        return Ok(new ApiResponse<IEnumerable<PaymentDto>>
        {
            Success = true,
            Message = "Payment history retrieved successfully.",
            Data = payments
        });
    }

    // GET: api/orders/{orderId}/payments/{paymentId}
    [HttpGet("{paymentId:int}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPaymentById(int orderId, int paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(CurrentUserId, orderId, paymentId, cancellationToken);

        if (payment == null)
        {
            return NotFound(new ApiResponse<PaymentDto>
            {
                Success = false,
                Message = "Payment record not found."
            });
        }

        return Ok(new ApiResponse<PaymentDto>
        {
            Success = true,
            Message = "Payment details retrieved successfully.",
            Data = payment
        });
    }
}