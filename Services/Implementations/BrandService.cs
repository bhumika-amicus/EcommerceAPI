using AutoMapper;
using EcommerceAPI.DTOs.Brands;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<BrandService> _logger;

    public BrandService(IBrandRepository brandRepository, IMapper mapper, IMemoryCache memoryCache, ILogger<BrandService> logger)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IEnumerable<BrandDto>> GetAllBrandsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all brands.");
        var cacheKey = "brands:all";
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<BrandDto>? cachedBrands))
        {
            _logger.LogInformation("Returning brands from cache.");
            return cachedBrands!;
        }

        var brands = await _brandRepository.GetAllBrandsAsync(cancellationToken);
        var brandDtos = _mapper.Map<IEnumerable<BrandDto>>(brands);

        _logger.LogInformation("Fetched {Count} brands from database.", brandDtos.Count());
        _memoryCache.Set(cacheKey, brandDtos, TimeSpan.FromMinutes(60));

        return brandDtos;
    }

    public async Task<BrandDto?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching brand with ID {BrandId}.", brandId);
        var cacheKey = $"brand:{brandId}";
        if (_memoryCache.TryGetValue(cacheKey, out BrandDto? cachedBrand))
        {
            _logger.LogInformation("Returning brand {BrandId} from cache.", brandId);
            return cachedBrand;
        }

        var brand = await _brandRepository.GetBrandByIdAsync(brandId, cancellationToken);
        var brandDto = _mapper.Map<BrandDto?>(brand);

        if (brandDto != null)
        {
            _logger.LogInformation("Fetched brand {BrandId} from database.", brandId);
            _memoryCache.Set(cacheKey, brandDto, TimeSpan.FromMinutes(60));
        }

        return brandDto;
    }
}
