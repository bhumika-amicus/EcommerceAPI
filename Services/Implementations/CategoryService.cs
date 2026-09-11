using AutoMapper;
using EcommerceAPI.DTOs.Categories;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, IMemoryCache memoryCache, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all categories.");
        var cacheKey = "categories:all";
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<CategoryDto>? cachedCategories))
        {
            _logger.LogInformation("Returning categories from cache.");
            return cachedCategories!;
        }

        var categories = await _categoryRepository.GetAllCategoriesAsync(cancellationToken);
        var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

        _logger.LogInformation("Fetched {Count} categories from database.", categoryDtos.Count());
        _memoryCache.Set(cacheKey, categoryDtos, TimeSpan.FromMinutes(60));

        return categoryDtos;
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching category with ID {CategoryId}.", categoryId);
        var cacheKey = $"category:{categoryId}";
        if (_memoryCache.TryGetValue(cacheKey, out CategoryDto? cachedCategory))
        {
            _logger.LogInformation("Returning category {CategoryId} from cache.", categoryId);
            return cachedCategory;
        }

        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId, cancellationToken);
        var categoryDto = _mapper.Map<CategoryDto?>(category);

        if (categoryDto != null)
        {
            _logger.LogInformation("Fetched category {CategoryId} from database.", categoryId);
            _memoryCache.Set(cacheKey, categoryDto, TimeSpan.FromMinutes(60));
        }

        return categoryDto;
    }
}
