using RecipeHub.Domain.Common;
using RecipeHub.Application.ViewModels.Favorite;

namespace RecipeHub.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<PaginationResult<FavoriteViewModel>> GetUserFavoritesAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<bool> IsFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task<bool> AddFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task<bool> RemoveFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task<int> GetFavoriteCountAsync(Guid recipeId, CancellationToken cancellationToken = default);
        Task<int> GetUserFavoriteCountAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
