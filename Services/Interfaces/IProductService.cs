using System;
using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Products;
using Microsoft.AspNetCore.Http;

namespace EcommerceAPI.Services;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(int productId , CancellationToken cancellationToken = default);

    Task<PagedResult<ProductDto>> GetAllProductsAsync(ProductQueryDto query , CancellationToken cancellationToken = default);

    Task<ProductDto?> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(int productId, UpdateProductDto dto , CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int productId, CancellationToken cancellationToken = default);

    Task<ProductAvailabilityDto?> GetProductAvailabilityAsync(int productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BatchStockValidationResultDto>> ValidateBatchStockAsync(IEnumerable<BatchStockCheckItemDto> items, CancellationToken cancellationToken = default);

    Task<bool> UploadProductImageAsync( int productId, IFormFile file, CancellationToken cancellationToken = default);

    Task<(Stream FileStream, string ContentType, string FileName)?> DownloadProductImageAsync(int productId, CancellationToken cancellationToken = default);


}