using AutoMapper;
using EcommerceAPI.DTOs.Brands;
using EcommerceAPI.Repositories;

namespace EcommerceAPI.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;

    public BrandService(IBrandRepository brandRepository, IMapper mapper)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BrandDto>> GetAllBrandsAsync(CancellationToken cancellationToken = default)
    {
        var brands = await _brandRepository.GetAllBrandsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<BrandDto>>(brands);
    }

    public async Task<BrandDto?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetBrandByIdAsync(brandId, cancellationToken);
        return _mapper.Map<BrandDto?>(brand);
    }
}
