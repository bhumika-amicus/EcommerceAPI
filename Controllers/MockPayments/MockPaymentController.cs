
using EcommerceAPI.DTOs.Payments;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.MockPayments;

[ApiController]
[Route("api/mock-payments")]
public class MockPaymentController : ControllerBase
{
    [HttpPost]
    public ActionResult<MockPaymentResponseDto> ProcessPayment( [FromBody] MockPaymentRequestDto request)
    {
        if (request.SimulateFailure)
        {
            return Ok(new MockPaymentResponseDto
            {
                Success = false,
                TransactionReference = string.Empty,
                Message = "Mock payment failed."
            });
        }

        var transactionReference = Guid.NewGuid().ToString();

        return Ok(new MockPaymentResponseDto
        {
            Success = true,
            TransactionReference = transactionReference,
            Message = "Mock payment processed successfully."
        });
    }
}

