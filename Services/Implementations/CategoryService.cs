using AutoMapper;
using EcommerceAPI.DTOs.Categories;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _memoryCache;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, IMemoryCache memoryCache)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _memoryCache = memoryCache;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "categories:all";
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<CategoryDto>? cachedCategories))
        {
            return cachedCategories!;
        }

        var categories = await _categoryRepository.GetAllCategoriesAsync(cancellationToken);
        var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

        _memoryCache.Set(cacheKey, categoryDtos, TimeSpan.FromMinutes(60));

        return categoryDtos;
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"category:{categoryId}";
        if (_memoryCache.TryGetValue(cacheKey, out CategoryDto? cachedCategory))
        {
            return cachedCategory;
        }

        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId, cancellationToken);
        var categoryDto = _mapper.Map<CategoryDto?>(category);

        if (categoryDto != null)
        {
            _memoryCache.Set(cacheKey, categoryDto, TimeSpan.FromMinutes(60));
        }

        return categoryDto;
    }
}
