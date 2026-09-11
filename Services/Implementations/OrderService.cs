
using System.Net;
using EcommerceAPI.Common;
using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.DTOs.Orders;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Orders;
using EcommerceAPI.Repositories;

using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartService _cartService;
    private readonly IProductService _productService;
    private readonly IShippingMethodService _shippingMethodService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<OrderService> _logger;
    private readonly IAddressService _addressService;

    public OrderService(
        IOrderRepository orderRepository, 
        ICartService cartService, 
        IProductService productService,  
        IShippingMethodService shippingMethodService,
        IAuditLogRepository auditLogRepository,
        ILogger<OrderService> logger,
       IAddressService addressService
        )
    {
        _orderRepository = orderRepository;
        _cartService = cartService;
        _productService = productService;
        _shippingMethodService = shippingMethodService;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
        _addressService = addressService;
    }

    public async Task<OrderDto> CreateOrderAsync( int customerId, CheckoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var cart = await _cartService.GetCartByCustomerIdAsync(customerId, cancellationToken);

        if (cart.Items.Count == 0) {
            _logger.LogWarning("Customer {CustomerId} attempted to checkout with an empty cart.", customerId);
            throw new BusinessException("Cannot create an order because the cart is empty."); 
        }

        var stockItems = cart.Items.Select(item => new BatchStockCheckItemDto { 
            ProductId = item.ProductId,
            RequestedQuantity = item.Quantity 
        }).ToList();
        
        var stockResults = await _productService.ValidateBatchStockAsync(stockItems, cancellationToken);
        var unavailableItems = stockResults.Where(x => !x.IsAvailable).ToList();
        
        if (unavailableItems.Any()) { 
            var message = string.Join("; ", unavailableItems.Select(x => x.Message));
            _logger.LogWarning("Customer {CustomerId} checkout failed due to unavailable products: {Message}", customerId, message);
            throw new BusinessException($"Some products are no longer available: {message}"); 
        }

        var shippingMethod = await _shippingMethodService.GetShippingMethodByIdAsync( request.ShippingMethodId, cancellationToken);

        if (shippingMethod == null)
        {
            throw new NotFoundException(
                $"Shipping method with ID {request.ShippingMethodId} was not found.");
        }


        var address = await _addressService.GetAddressByUserIdAsync( customerId, cancellationToken);

        if (address == null)
        {
            throw new BusinessException(
                "No shipping address found. Please add an address before placing the order.");
        }

        var shippingAddress = string.Join(", ", new[]
       {
            address.AddressLine1,
            address.AddressLine2,
            address.City,
            address.State,
            address.PostalCode,
            address.Country
        }.Where(x => !string.IsNullOrWhiteSpace(x)));


        var orderItems = cart.Items.Select(item => new CreateOrderItemModel{
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.CurrentPrice,
            Quantity = item.Quantity,
            LineTotal = item.CurrentPrice * item.Quantity 
        }).ToList();


        var subtotal = orderItems.Sum(x => x.LineTotal);

        var taxAmount = subtotal * 0.18m;

        var shippingFee = shippingMethod.Fee;

        var totalAmount = subtotal + shippingFee + taxAmount;


        var order = new CreateOrderModel
        {
            CustomerId = customerId,
            ShippingMethodId = request.ShippingMethodId,
            ShippingAddress = shippingAddress,
            Subtotal = subtotal,
            ShippingFee = shippingFee,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            Items = orderItems
        };

        var orderId = await _orderRepository.CreateOrderAsync( order, cancellationToken);

        var createdOrder = await _orderRepository.GetOrderByIdAsync( orderId, cancellationToken);

        if (createdOrder == null)
        {
            throw new InvalidOperationException(
                "Order was created but could not be retrieved.");
        }

        // Clear the cart after successful order creation
        await _cartService.ClearCartAsync(customerId, cancellationToken);
        _logger.LogInformation("Cart cleared for customer {CustomerId} after creating order {OrderId}.", customerId, orderId);

        // Audit Logging
        await _auditLogRepository.LogAsync(
            customerId.ToString(),
            "CreateOrder",
            "Order",
            orderId.ToString(),
            null,
            null,
            200,
            cancellationToken);

        return createdOrder;
    }

    public async Task<OrderDto?> GetOrderByIdAsync( int customerId, int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderByIdAsync(
            orderId,
            cancellationToken);

        if (order == null)
        {
            return null;
        }

        // Important:
        // A customer should only be able to retrieve
        // their own order.
        if (order.CustomerId != customerId)
        {
            throw new NotFoundException(
                $"Order with ID {orderId} was not found.");
        }

        return order;
    }

    public async Task<PagedResult<OrderDto>> GetCustomerOrdersPagedAsync(int customerId, OrderQueryDto query, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetCustomerOrdersPagedAsync(customerId, query, cancellationToken);
    }

    public async Task<CartDto> ReorderAsync(int customerId, int orderId, CancellationToken cancellationToken = default)
    {
        // 1. Fetch the past order
        var order = await GetOrderByIdAsync(customerId, orderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} was not found.");
        }

        // 2. Make sure order has line items
        if (order.Items == null || order.Items.Count == 0)
        {
            throw new BusinessException($"Order with ID {orderId} has no items to reorder.");
        }

        // 3. Add each item into customer's active cart
        foreach (var item in order.Items)
        {
            await _cartService.AddItemToCartAsync(
                customerId,
                new AddToCartDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                },
                cancellationToken);
        }

        // 4. Return updated cart
        return await _cartService.GetCartByCustomerIdAsync(customerId, cancellationToken);
    }

    public async Task<OrderDto> CancelOrderAsync(int customerId, int orderId, CancellationToken cancellationToken = default)
    {
        await _orderRepository.CancelOrderAsync(customerId, orderId, cancellationToken);

        var updatedOrder = await GetOrderByIdAsync(customerId, orderId, cancellationToken);
        if (updatedOrder == null)
        {
            throw new NotFoundException($"Order with ID {orderId} was not found.");
        }

        _logger.LogInformation("Order {OrderId} cancelled by customer {CustomerId}.", orderId, customerId);

        // Audit Logging
        await _auditLogRepository.LogAsync(
            customerId.ToString(),
            "CancelOrder",
            "Order",
            orderId.ToString(),
            null,
            null,
            200,
            cancellationToken);

        return updatedOrder;
    }
}

