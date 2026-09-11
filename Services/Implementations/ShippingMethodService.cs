using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class ShippingMethodService : IShippingMethodService
{
    private readonly IShippingMethodRepository _shippingMethodRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ShippingMethodService> _logger;

    public ShippingMethodService( IShippingMethodRepository shippingMethodRepository, IMemoryCache memoryCache, ILogger<ShippingMethodService> logger)
    {
        _shippingMethodRepository = shippingMethodRepository;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<ShippingMethodDto?> GetShippingMethodByIdAsync( int shippingMethodId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching shipping method with ID {ShippingMethodId}.", shippingMethodId);
        var cacheKey = $"shippingmethod:{shippingMethodId}";
        if (_memoryCache.TryGetValue(cacheKey, out ShippingMethodDto? cachedMethod))
        {
            _logger.LogInformation("Returning shipping method {ShippingMethodId} from cache.", shippingMethodId);
            return cachedMethod;
        }

        var method = await _shippingMethodRepository.GetShippingMethodByIdAsync( shippingMethodId, cancellationToken);
        
        if (method != null)
        {
            _logger.LogInformation("Fetched shipping method {ShippingMethodId} from database.", shippingMethodId);
            _memoryCache.Set(cacheKey, method, TimeSpan.FromMinutes(60));
        }

        return method;
    }

    public async Task<IEnumerable<ShippingMethodDto>> GetAllShippingMethodsAsync( CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all shipping methods.");
        var cacheKey = "shippingmethods:all";
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<ShippingMethodDto>? cachedMethods))
        {
            _logger.LogInformation("Returning shipping methods from cache.");
            return cachedMethods!;
        }

        var methods = await _shippingMethodRepository.GetAllShippingMethodsAsync( cancellationToken);
        _logger.LogInformation("Fetched {Count} shipping methods from database.", methods.Count());
        _memoryCache.Set(cacheKey, methods, TimeSpan.FromMinutes(60));

        return methods;
    }
}