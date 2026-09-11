using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class CheckoutService : ICheckoutService
{
    private readonly ICartService _cartService;
    private readonly IProductService _productService;
    private readonly IShippingMethodService _shippingMethodService;
    private readonly IAddressService _addressService;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService( ICartService cartService, IProductService productService, IShippingMethodService shippingMethodService,IAddressService addressService, ILogger<CheckoutService> logger)
    {
        _cartService = cartService;
        _productService = productService;
        _shippingMethodService = shippingMethodService;
        _addressService = addressService;
        _logger = logger;
    }


    public async Task<CheckoutDto> PreviewCheckoutAsync( int customerId, CheckoutRequestDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting checkout preview for customer {CustomerId}.", customerId);
        // 1. Get cart
        var cart = await _cartService.GetCartByCustomerIdAsync( customerId, cancellationToken);
        // 2. Check empty cart
        if (cart.Items.Count == 0)
        {
            _logger.LogWarning("Checkout preview failed for customer {CustomerId}: Cart is empty.", customerId);
            throw new BusinessException( "Cannot proceed with checkout because the cart is empty.");
        }

        // 3. Prepare stock validation request
        var stockItems = cart.Items.Select(item =>new BatchStockCheckItemDto{ ProductId = item.ProductId, RequestedQuantity = item.Quantity});

        // 4. Validate stock
        var stockResults = await _productService.ValidateBatchStockAsync( stockItems, cancellationToken);

        // 5. Find unavailable products
        var unavailableItems = stockResults.Where(x => !x.IsAvailable).ToList();

        // 6. Stop checkout if stock is insufficient
        if (unavailableItems.Any())
        {
            var message = string.Join( "; ",unavailableItems.Select(x => x.Message));
            _logger.LogWarning("Checkout preview failed for customer {CustomerId}: Stock issues - {Message}", customerId, message);
            throw new BusinessException( $"Checkout cannot proceed due to stock issues: {message}");
        }

        // 7. Build checkout items using CURRENT prices

        var checkoutItems = cart.Items.Select(item => new CheckoutItemDto
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.CurrentPrice,
            Quantity = item.Quantity,
            LineTotal = item.CurrentPrice * item.Quantity

        }).ToList();

        // 8. Calculate subtotal
        var subtotal = checkoutItems.Sum(item => item.LineTotal);

        // 9. Detect price changes
        var priceChangeMessages = cart.Items.Where(item => item.HasPriceChanged).Select(item => item.PriceChangeMessage).ToList();


        // 10. Get selected shipping method 
        var shippingMethod =
            await _shippingMethodService.GetShippingMethodByIdAsync( dto.ShippingMethodId,  cancellationToken);

        // 11. Shipping method must exist and be active
        if (shippingMethod == null)
        {
            _logger.LogWarning("Checkout preview failed for customer {CustomerId}: Shipping method {ShippingMethodId} not found.", customerId, dto.ShippingMethodId);
            throw new NotFoundException(
                $"Shipping method with ID {dto.ShippingMethodId} " + "does not exist or is currently unavailable.");
        }

        // 12. Get shipping fee from database
        var shippingFee = shippingMethod.Fee;

        // 13. Calculate tax
        const decimal taxRate = 0.18m;

        var taxAmount = subtotal * taxRate;

        // 14. Calculate final total
        var totalAmount = subtotal + shippingFee + taxAmount;

        //15. get user address
        var address = await _addressService.GetAddressByUserIdAsync(customerId, cancellationToken);

        if (address == null)
        {
            _logger.LogWarning("Checkout preview failed for customer {CustomerId}: No shipping address found.", customerId);
            throw new BusinessException( "No shipping address found. Please add an address before checkout.");
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


        // 16. Return checkout preview
        return new CheckoutDto
        {
            Items = checkoutItems,
            ShippingAddress = shippingAddress,
            ShippingMethodId = shippingMethod.ShippingMethodId,
            ShippingMethod = shippingMethod.Name,
            Subtotal = subtotal,
            ShippingFee = shippingFee,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            PriceChangeMessages = priceChangeMessages
        };
    
        
    }
}