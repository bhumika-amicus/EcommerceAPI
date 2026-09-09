
using EcommerceAPI.DTOs.Payments;

namespace EcommerceAPI.Services;

public interface IPaymentService
{
    Task<PaymentDto> ProcessPaymentAsync( int customerId, int orderId, PaymentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(int customerId, int orderId, CancellationToken cancellationToken = default);

    Task<PaymentDto?> GetPaymentByIdAsync(int customerId, int orderId, int paymentId, CancellationToken cancellationToken = default);
}

