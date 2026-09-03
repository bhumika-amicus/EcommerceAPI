using AutoMapper;
using EcommerceAPI.DTOs.Categories;
using EcommerceAPI.Repositories;
using EcommerceAPI.Services;

namespace EcommerceAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId, cancellationToken);
        return _mapper.Map<CategoryDto?>(category);
    }
}
