using RecipeHub.Domain.Common;
using RecipeHub.Domain.Entities;

namespace RecipeHub.Domain.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<PaginationResult<Favorite>> GetUserFavoritesPagedAsync(Guid userId, PaginationRequest request, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task<Favorite?> GetFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default);
        Task RemoveAsync(Favorite favorite, CancellationToken cancellationToken = default);
        Task<int> CountByRecipeAsync(Guid recipeId, CancellationToken cancellationToken = default);
        Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
