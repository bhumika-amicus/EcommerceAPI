
using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.DTOs.Orders;

namespace EcommerceAPI.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(int customerId, CheckoutRequestDto request, CancellationToken cancellationToken = default); 
    Task<OrderDto?> GetOrderByIdAsync(int customerId, int orderId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetCustomerOrdersPagedAsync(int customerId, OrderQueryDto query, CancellationToken cancellationToken = default);
    Task<CartDto> ReorderAsync(int customerId, int orderId, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelOrderAsync(int customerId, int orderId, CancellationToken cancellationToken = default);
}

