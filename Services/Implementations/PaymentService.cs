using System;
using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.DTOs.Payments;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMockPaymentClient _mockPaymentClient;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IErrorLogRepository _errorLogRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository, 
        IOrderRepository orderRepository, 
        IMockPaymentClient mockPaymentClient,
        IAuditLogRepository auditLogRepository,
        IErrorLogRepository errorLogRepository,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _mockPaymentClient = mockPaymentClient;
        _auditLogRepository = auditLogRepository;
        _errorLogRepository = errorLogRepository;
        _logger = logger;
    }

    public async Task<PaymentDto> ProcessPaymentAsync(int customerId, int orderId, PaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        // 1. Get the order
        var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} was not found.");
        }

        // 2. Make sure the order belongs to the current customer

        if (order.CustomerId != customerId)
        {
            throw new NotFoundException($"Order with ID {orderId} was not found.");
        }
        // 3. Order must be in Pending status

        if (!string.Equals( order.OrderStatus, "Pending", StringComparison.OrdinalIgnoreCase)) {
            _logger.LogWarning("Customer {CustomerId} attempted to pay for order {OrderId} but it is in status '{OrderStatus}'.", customerId, orderId, order.OrderStatus);
            throw new BusinessException( $"Order with ID {orderId} cannot be paid because its current status is '{order.OrderStatus}'."); 
        }

        // 5. Create a Pending payment record
        var payment = await _paymentRepository.CreatePaymentAsync( order.OrderId, order.TotalAmount, request.PaymentMethod, cancellationToken);

        // 6. Call the Mock Payment API
        var mockResult = await _mockPaymentClient.ProcessPaymentAsync(order.TotalAmount, request.PaymentMethod, request.SimulateFailure, cancellationToken);

        // 7. Handle mock payment result (Successful or Failed)
        string paymentStatus = mockResult.Success ? "Successful" : "Failed";
        string? transactionReference = mockResult.Success ? mockResult.TransactionReference : null;

        try
        {
            await _paymentRepository.ProcessPaymentResultAsync(payment.PaymentId, orderId, paymentStatus, transactionReference, cancellationToken);
        }
        catch (Exception ex)
        {
            // Orphaned Payment Risk: If DB update fails after a successful mock charge
            if (mockResult.Success)
            {
                _logger.LogCritical(ex, "ORPHANED PAYMENT: Card was charged successfully (Ref: {Ref}) but Database update failed for Order {OrderId}!", transactionReference, orderId);
                await _errorLogRepository.LogErrorAsync(
                    "Error",
                    $"Card charged (Ref: {transactionReference}) but DB update failed for Order {orderId}. Exception: {ex.Message}",
                    "OrphanedPayment",
                    ex.StackTrace,
                    ex.InnerException?.Message,
                    null,
                    null,
                    500,
                    cancellationToken);
            }
            throw;
        }

        var updatedPayment = await _paymentRepository.GetPaymentByIdAsync(payment.PaymentId, cancellationToken);

        if (mockResult.Success)
        {
            _logger.LogInformation("Payment successful for order {OrderId}. Transaction Ref: {Ref}", orderId, transactionReference);
            await _auditLogRepository.LogAsync(
                customerId.ToString(),
                "ProcessPayment",
                "Payment",
                payment.PaymentId.ToString(),
                null,
                null,
                200,
                cancellationToken);
        }
        else
        {
            _logger.LogWarning("Payment failed for order {OrderId} for customer {CustomerId}.", orderId, customerId);
            await _errorLogRepository.LogErrorAsync(
                "Warning",
                $"Payment declined for order {orderId}",
                "PaymentFailed",
                null,
                null,
                null,
                null,
                400,
                cancellationToken);
        }

        return updatedPayment!;
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(int customerId, int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        if (order == null || order.CustomerId != customerId)
        {
            throw new NotFoundException($"Order with ID {orderId} was not found.");
        }

        return await _paymentRepository.GetPaymentsByOrderIdAsync(orderId, cancellationToken);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(int customerId, int orderId, int paymentId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        if (order == null || order.CustomerId != customerId)
        {
            throw new NotFoundException($"Order with ID {orderId} was not found.");
        }

        var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId, cancellationToken);
        if (payment == null || payment.OrderId != orderId)
        {
            throw new NotFoundException($"Payment with ID {paymentId} for order {orderId} was not found.");
        }

        return payment;
    }
}