using RecipeHub.Domain.Common;
using RecipeHub.Application.ViewModels.Category;

namespace RecipeHub.Application.Interfaces;

public interface ICategoryService
{
    // Métodos básicos
    Task<IEnumerable<CategoryListViewModel>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<PaginationResult<CategoryListViewModel>> GetCategoriesPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<CategoryDetailViewModel?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryDetailViewModel?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Guid> AddCategoryAsync(CategoryCreateViewModel request, CancellationToken cancellationToken = default);
    Task<bool> UpdateCategoryAsync(Guid id, CategoryUpdateViewModel request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Métodos de contagem
    Task<int> GetTotalCategoriesCountAsync(CancellationToken cancellationToken = default);
}
