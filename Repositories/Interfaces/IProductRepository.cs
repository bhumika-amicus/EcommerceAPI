using System;
using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Models;

namespace EcommerceAPI.Repositories;

public interface IProductRepository
{
    Task<PagedResult<ProductDetailModel>> GetAllProductsAsync(ProductQueryDto query, CancellationToken cancellationToken = default);

    Task<ProductDetailModel?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default);

    // Product CRUD Methods
    Task<int> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(int productId, UpdateProductDto dto , CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int productId, CancellationToken cancellationToken = default);

    // Product Availability Methods
    Task<ProductAvailabilityDto?> GetProductAvailabilityAsync(int productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BatchStockValidationResultDto>> ValidateBatchStockAsync(IEnumerable<BatchStockCheckItemDto> items, CancellationToken cancellationToken = default);

    //method for file upload
    
    Task<bool> UpdateImagePathAsync( int productId, string imagePath, CancellationToken cancellationToken = default);

    //bulk product upload
    Task<BulkCreateProductResponseDto> BulkCreateProductsAsync( List<CreateProductDto> products, CancellationToken cancellationToken = default);

    //bulk inventory update
    Task<BulkInventoryUpdateResponseDto> BulkUpdateInventoryAsync(List<BulkInventoryUpdateItemDto> items, CancellationToken cancellationToken = default);
}