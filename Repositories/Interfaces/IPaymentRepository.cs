
using System;
using EcommerceAPI.DTOs.Payments;

namespace EcommerceAPI.Repositories;

public interface IPaymentRepository
{
    Task<PaymentDto> CreatePaymentAsync( int orderId , decimal amount, string paymentMethod, CancellationToken cancellationToken = default);

    Task ProcessPaymentResultAsync(
    int paymentId,
    int orderId,
    string paymentStatus,
    string? transactionReference,
    CancellationToken cancellationToken = default);


    Task<PaymentDto?> GetPaymentByIdAsync( int paymentId, CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync( int orderId, CancellationToken cancellationToken = default);
}

