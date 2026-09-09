using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Orders;
using EcommerceAPI.Models.Orders;
namespace EcommerceAPI.Repositories; 

public interface IOrderRepository {
    Task<int> CreateOrderAsync(CreateOrderModel dto, CancellationToken cancellationToken = default);

    Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);

    Task<PagedResult<OrderDto>> GetCustomerOrdersPagedAsync(int customerId, OrderQueryDto query, CancellationToken cancellationToken = default);

    Task<bool> CancelOrderAsync(int customerId, int orderId, CancellationToken cancellationToken = default);
}