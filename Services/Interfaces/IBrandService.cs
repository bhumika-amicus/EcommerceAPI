using EcommerceAPI.DTOs.Brands;

namespace EcommerceAPI.Services;

public interface IBrandService
{
    Task<IEnumerable<BrandDto>> GetAllBrandsAsync(CancellationToken cancellationToken = default);

    Task<BrandDto?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default);
}
