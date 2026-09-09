using AutoMapper;
using EcommerceAPI.DTOs.Brands;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceAPI.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _memoryCache;

    public BrandService(IBrandRepository brandRepository, IMapper mapper, IMemoryCache memoryCache)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
        _memoryCache = memoryCache;
    }

    public async Task<IEnumerable<BrandDto>> GetAllBrandsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "brands:all";
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<BrandDto>? cachedBrands))
        {
            return cachedBrands!;
        }

        var brands = await _brandRepository.GetAllBrandsAsync(cancellationToken);
        var brandDtos = _mapper.Map<IEnumerable<BrandDto>>(brands);

        _memoryCache.Set(cacheKey, brandDtos, TimeSpan.FromMinutes(60));

        return brandDtos;
    }

    public async Task<BrandDto?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"brand:{brandId}";
        if (_memoryCache.TryGetValue(cacheKey, out BrandDto? cachedBrand))
        {
            return cachedBrand;
        }

        var brand = await _brandRepository.GetBrandByIdAsync(brandId, cancellationToken);
        var brandDto = _mapper.Map<BrandDto?>(brand);

        if (brandDto != null)
        {
            _memoryCache.Set(cacheKey, brandDto, TimeSpan.FromMinutes(60));
        }

        return brandDto;
    }
}
