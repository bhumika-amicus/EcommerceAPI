using AutoMapper;
using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, IMapper mapper, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var productModel = await _productRepository.GetProductByIdAsync(productId, cancellationToken);
        return _mapper.Map<ProductDto?>(productModel);
    }

    public async Task<PagedResult<ProductDto>> GetAllProductsAsync(ProductQueryDto query, CancellationToken cancellationToken = default)
    {
        var pagedModels = await _productRepository.GetAllProductsAsync(query, cancellationToken);
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(pagedModels.Items);

        return new PagedResult<ProductDto>
        {
            Items = productDtos,
            PageNumber = pagedModels.PageNumber,
            PageSize = pagedModels.PageSize,
            TotalCount = pagedModels.TotalCount
        };
    }

    public async Task<ProductDto?> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var newProductId = await _productRepository.CreateProductAsync(dto, cancellationToken);
        _logger.LogInformation("Product Created: New ProductId {ProductId} created with Name {Name}", newProductId, dto.Name);
        return await GetProductByIdAsync(newProductId, cancellationToken);
    }

    public async Task<bool> UpdateProductAsync(int productId, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var isUpdated = await _productRepository.UpdateProductAsync(productId, dto, cancellationToken);
        if (isUpdated)
        {
            _logger.LogInformation("Product Updated: ProductId {ProductId} updated successfully", productId);
        }
        else
        {
            _logger.LogWarning("Product Update Failed: ProductId {ProductId} not found", productId);
        }
        return isUpdated;
    }

    public async Task<bool> DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var isDeleted = await _productRepository.DeleteProductAsync(productId, cancellationToken);
        if (isDeleted)
        {
            _logger.LogInformation("Product Deleted: ProductId {ProductId} deleted successfully", productId);
        }
        else
        {
            _logger.LogWarning("Product Delete Failed: ProductId {ProductId} not found", productId);
        }
        return isDeleted;
    }

    public async Task<ProductAvailabilityDto?> GetProductAvailabilityAsync(int productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking Stock Availability for ProductId {ProductId}", productId);
        return await _productRepository.GetProductAvailabilityAsync(productId, cancellationToken);
    }

    public async Task<IEnumerable<BatchStockValidationResultDto>> ValidateBatchStockAsync(IEnumerable<BatchStockCheckItemDto> items, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Batch Stock Validation for {Count} items", items.Count());
        return await _productRepository.ValidateBatchStockAsync(items, cancellationToken);
    }
}
