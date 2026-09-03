using EcommerceAPI.DTOs.Cart;
namespace EcommerceAPI.Services;
public interface ICartService
{
    Task<CartDto> GetCartByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<int> GetCartItemCountAsync(int customerId, CancellationToken cancellationToken = default);
    Task<decimal> GetCartSubtotalAsync(int customerId, CancellationToken cancellationToken = default);
    Task<CartDto> AddItemToCartAsync(int customerId, AddToCartDto dto, CancellationToken cancellationToken = default);
    Task<CartDto> UpdateCartItemQuantityAsync(int customerId, int productId, UpdateCartItemDto dto, CancellationToken cancellationToken = default);
    Task<CartDto> RemoveCartItemAsync(int customerId, int productId, CancellationToken cancellationToken = default);
    Task<CartDto> ClearCartAsync(int customerId, CancellationToken cancellationToken = default);
}