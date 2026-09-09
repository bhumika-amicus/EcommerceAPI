using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceAPI.Services;

public class ShippingMethodService : IShippingMethodService
{
    private readonly IShippingMethodRepository _shippingMethodRepository;
    private readonly IMemoryCache _memoryCache;

    public ShippingMethodService( IShippingMethodRepository shippingMethodRepository, IMemoryCache memoryCache)
    {
        _shippingMethodRepository = shippingMethodRepository;
        _memoryCache = memoryCache;
    }

    public async Task<ShippingMethodDto?> GetShippingMethodByIdAsync( int shippingMethodId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"shippingmethod:{shippingMethodId}";
        if (_memoryCache.TryGetValue(cacheKey, out ShippingMethodDto? cachedMethod))
        {
            return cachedMethod;
        }

        var method = await _shippingMethodRepository.GetShippingMethodByIdAsync( shippingMethodId, cancellationToken);
        
        if (method != null)
        {
            _memoryCache.Set(cacheKey, method, TimeSpan.FromMinutes(60));
        }

        return method;
    }

    public async Task<IEnumerable<ShippingMethodDto>> GetAllShippingMethodsAsync( CancellationToken cancellationToken = default)
    {
        var cacheKey = "shippingmethods:all";
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<ShippingMethodDto>? cachedMethods))
        {
            return cachedMethods!;
        }

        var methods = await _shippingMethodRepository.GetAllShippingMethodsAsync( cancellationToken);
        _memoryCache.Set(cacheKey, methods, TimeSpan.FromMinutes(60));

        return methods;
    }
}