using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Products;

namespace EcommerceAPI.Services;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(int productId , CancellationToken cancellationToken = default);

    Task<PagedResult<ProductDto>> GetAllProductsAsync(ProductQueryDto query , CancellationToken cancellationToken = default);

    // Add CRUD methods:
    Task<ProductDto?> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(int productId, UpdateProductDto dto , CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int productId, CancellationToken cancellationToken = default);

    Task<ProductAvailabilityDto?> GetProductAvailabilityAsync(int productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BatchStockValidationResultDto>> ValidateBatchStockAsync(IEnumerable<BatchStockCheckItemDto> items, CancellationToken cancellationToken = default);
}