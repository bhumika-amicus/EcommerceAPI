using EcommerceAPI.Models;

namespace EcommerceAPI.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

    Task<Category?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default);
}