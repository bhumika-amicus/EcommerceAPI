using EcommerceAPI.DTOs.Cart;
namespace EcommerceAPI.Repositories;
public interface ICartRepository
{
    Task<CartDto> GetCartByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<int> GetCartItemCountAsync(int customerId, CancellationToken cancellationToken = default);
    Task<decimal> GetCartSubtotalAsync(int customerId, CancellationToken cancellationToken = default);
    Task AddItemToCartAsync(int customerId, AddToCartDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateCartItemQuantityAsync(int customerId, int productId, UpdateCartItemDto dto, CancellationToken cancellationToken = default);
    Task<bool> RemoveCartItemAsync(int customerId, int productId, CancellationToken cancellationToken = default);
    Task<bool> ClearCartAsync(int customerId, CancellationToken cancellationToken = default);
}