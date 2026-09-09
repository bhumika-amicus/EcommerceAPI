using EcommerceAPI.Common;
using EcommerceAPI.DTOs.Categories;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Categories;

[Route("api/categories")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ResponseCache(Duration = 120)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);

        return Ok(new ApiResponse<IEnumerable<CategoryDto>>
        {
            Success = true,
            Message = "Categories retrieved successfully.",
            Data = categories
        });
    }

    [HttpGet("{categoryId:int}")]
    [ResponseCache(Duration = 120)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int categoryId, CancellationToken cancellationToken)
    {
        if (categoryId <= 0)
        {
            return BadRequest(new ApiResponse<CategoryDto>
            {
                Success = false,
                Message = "Category ID must be greater than 0."
            });
        }

        var category = await _categoryService.GetCategoryByIdAsync(categoryId, cancellationToken);

        if (category == null)
        {
            return NotFound(new ApiResponse<CategoryDto>
            {
                Success = false,
                Message = "Category not found."
            });
        }

        return Ok(new ApiResponse<CategoryDto>
        {
            Success = true,
            Message = "Category retrieved successfully.",
            Data = category
        });
    }
}