using EcommerceAPI.DTOs.ProductPrices;

namespace EcommerceAPI.Services;

public interface IProductPriceService
{
    Task<IEnumerable<ProductPriceDto>> GetAllProductPricesAsync(CancellationToken cancellationToken = default);

    Task<ProductPriceDto?> GetProductPriceByProductIdAsync(int productId, CancellationToken cancellationToken = default);
}