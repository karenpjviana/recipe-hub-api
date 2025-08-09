using RecipeHub.Domain.Common;
using RecipeHub.Application.ViewModels.Recipe;
using RecipeHub.Application.ViewModels.Recipes;

namespace RecipeHub.Application.Interfaces;

public interface IRecipeService
{
    // ✅ MÉTODO UNIFICADO (substitui vários endpoints)
    Task<PaginationResult<RecipeListViewModel>> GetRecipesAsync(RecipeFilterRequest filter, CancellationToken cancellationToken = default);
    
    // Métodos básicos
    Task<RecipeDetailViewModel?> GetRecipeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RecipeDetailViewModel?> GetRecipeBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Guid> AddRecipeAsync(RecipeCreateViewModel request, CancellationToken cancellationToken = default);
    Task<bool> UpdateRecipeAsync(Guid id, RecipeUpdateViewModel request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Métodos de contagem
    Task<int> GetTotalRecipesCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetPublishedRecipesCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetUserRecipesCountAsync(Guid userId, CancellationToken cancellationToken = default);
}