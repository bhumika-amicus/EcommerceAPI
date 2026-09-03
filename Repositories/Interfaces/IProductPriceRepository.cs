using EcommerceAPI.Models;

namespace EcommerceAPI.Repositories;

public interface IProductPriceRepository
{
    Task<IEnumerable<ProductPrice>> GetAllProductPricesAsync(CancellationToken cancellationToken = default);

    Task<ProductPrice?> GetProductPriceByProductIdAsync(int productId, CancellationToken cancellationToken = default);
}