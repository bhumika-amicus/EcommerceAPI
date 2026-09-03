using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.DTOs.Cart;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository, ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<CartDto> GetCartByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving Shopping Cart for Customer {CustomerId}", customerId);
        return await _cartRepository.GetCartByCustomerIdAsync(customerId, cancellationToken);
    }

    public async Task<int> GetCartItemCountAsync(int customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving Cart Item Count for Customer {CustomerId}", customerId);
        return await _cartRepository.GetCartItemCountAsync(customerId, cancellationToken);
    }

    public async Task<decimal> GetCartSubtotalAsync(int customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving Cart Subtotal for Customer {CustomerId}", customerId);
        return await _cartRepository.GetCartSubtotalAsync(customerId, cancellationToken);
    }

    public async Task<CartDto> AddItemToCartAsync(int customerId, AddToCartDto dto, CancellationToken cancellationToken = default)
    {
        // Stock & Product Existence Check
        var availability = await _productRepository.GetProductAvailabilityAsync(dto.ProductId, cancellationToken);
        if (availability == null)
        {
            _logger.LogError("Cart Add Item Failed: Product ID {ProductId} not found for Customer {CustomerId}", dto.ProductId, customerId);
            throw new NotFoundException($"Product with ID {dto.ProductId} does not exist.");
        }

        if (dto.Quantity > availability.StockQuantity)
        {
            _logger.LogWarning("Cart Stock Shortage Warning: Customer {CustomerId} requested {Quantity} of ProductId {ProductId}, but only {AvailableStock} in stock", 
                customerId, dto.Quantity, dto.ProductId, availability.StockQuantity);

            throw new BusinessException($"Cannot add {dto.Quantity} items to cart. Only {availability.StockQuantity} available in stock.");
        }

        // Add to Cart Database
        await _cartRepository.AddItemToCartAsync(customerId, dto, cancellationToken);

        _logger.LogInformation("Cart Item Added: Added {Quantity} units of ProductId {ProductId} to Cart for Customer {CustomerId}", 
            dto.Quantity, dto.ProductId, customerId);

       
        return await GetCartByCustomerIdAsync(customerId, cancellationToken);
    }

    public async Task<CartDto> UpdateCartItemQuantityAsync(int customerId, int productId, UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        // Stock Check
        var availability = await _productRepository.GetProductAvailabilityAsync(productId, cancellationToken);
        if (availability == null)
        {
            _logger.LogError("Cart Update Quantity Failed: Product ID {ProductId} not found for Customer {CustomerId}", productId, customerId);
            throw new NotFoundException($"Product with ID {productId} does not exist.");
        }

        if (dto.Quantity > availability.StockQuantity)
        {
            _logger.LogWarning("Cart Stock Shortage Warning: Customer {CustomerId} attempted to set quantity to {Quantity} for ProductId {ProductId}, but only {AvailableStock} in stock", 
                customerId, dto.Quantity, productId, availability.StockQuantity);

            throw new BusinessException($"Cannot set quantity to {dto.Quantity}. Only {availability.StockQuantity} available in stock.");
        }

        //  Update Cart Item Quantity
        await _cartRepository.UpdateCartItemQuantityAsync(customerId, productId, dto, cancellationToken);

        _logger.LogInformation("Cart Item Quantity Updated: Customer {CustomerId} updated ProductId {ProductId} quantity to {Quantity}", 
            customerId, productId, dto.Quantity);

        //  Return updated CartDto
        return await GetCartByCustomerIdAsync(customerId, cancellationToken);
    }

    public async Task<CartDto> RemoveCartItemAsync(int customerId, int productId, CancellationToken cancellationToken = default)
    {
        await _cartRepository.RemoveCartItemAsync(customerId, productId, cancellationToken);

        _logger.LogInformation("Cart Item Removed: ProductId {ProductId} removed from Cart for Customer {CustomerId}", 
            productId, customerId);

        return await GetCartByCustomerIdAsync(customerId, cancellationToken);
    }

    public async Task<CartDto> ClearCartAsync(int customerId, CancellationToken cancellationToken = default)
    {
        await _cartRepository.ClearCartAsync(customerId, cancellationToken);

        _logger.LogInformation("Cart Cleared: All items removed from Cart for Customer {CustomerId}", customerId);

        return await GetCartByCustomerIdAsync(customerId, cancellationToken);
    }
}
