using EcommerceAPI.Models;

namespace EcommerceAPI.Repositories;

public interface IBrandRepository
{
    Task<IEnumerable<Brand>> GetAllBrandsAsync(CancellationToken cancellationToken = default);

    Task<Brand?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default);
}