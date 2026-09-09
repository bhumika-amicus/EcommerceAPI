using System;
using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.DTOs.Payments;
using EcommerceAPI.Repositories;

namespace EcommerceAPI.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMockPaymentClient _mockPaymentClient;
    public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IMockPaymentClient mockPaymentClient)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _mockPaymentClient = mockPaymentClient;
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
            throw new BusinessException( $"Order with ID {orderId} cannot be paid because its current status is '{order.OrderStatus}'."); 
        }

        // 5. Create a Pending payment record
        
        var payment = await _paymentRepository.CreatePaymentAsync( order.OrderId, order.TotalAmount, request.PaymentMethod, cancellationToken);

        // 6. Call the Mock Payment API
        var mockResult = await _mockPaymentClient.ProcessPaymentAsync(order.TotalAmount, request.PaymentMethod, request.SimulateFailure, cancellationToken);

        // 7. Handle mock payment result (Successful or Failed)
        string paymentStatus = mockResult.Success ? "Successful" : "Failed";
        string? transactionReference = mockResult.Success ? mockResult.TransactionReference : null;

        await _paymentRepository.ProcessPaymentResultAsync(payment.PaymentId, orderId, paymentStatus, transactionReference, cancellationToken);

        var updatedPayment = await _paymentRepository.GetPaymentByIdAsync(payment.PaymentId, cancellationToken);

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