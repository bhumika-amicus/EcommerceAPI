
using EcommerceAPI.DTOs.Payments;

namespace EcommerceAPI.Services;

public interface IMockPaymentClient
{
    Task<MockPaymentResponseDto> ProcessPaymentAsync(decimal amount, string paymentMethod, bool simulateFailure = false, CancellationToken cancellationToken = default);
}

