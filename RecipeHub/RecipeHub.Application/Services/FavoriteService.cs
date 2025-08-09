using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Favorite;
using RecipeHub.Application.ViewModels.Recipe;
using RecipeHub.Domain.Entities;
using RecipeHub.Domain.Interfaces;

namespace RecipeHub.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepo;
        private readonly IRepositoryBase<Recipe> _recipeRepo;
        private readonly IUnitOfWork _uow;

        public FavoriteService(
            IFavoriteRepository favoriteRepo,
            IRepositoryBase<Recipe> recipeRepo,
            IUnitOfWork uow)
        {
            _favoriteRepo = favoriteRepo;
            _recipeRepo = recipeRepo;
            _uow = uow;
        }

        public async Task<PaginationResult<FavoriteViewModel>> GetUserFavoritesAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize };
                var pagedFavorites = await _favoriteRepo.GetUserFavoritesPagedAsync(userId, request, cancellationToken);

                var viewModels = pagedFavorites.Items.Select(ToViewModel).ToList();
                
                return new PaginationResult<FavoriteViewModel>(
                    viewModels, 
                    pagedFavorites.TotalItems, 
                    pagedFavorites.PageNumber, 
                    pagedFavorites.PageSize
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao buscar favoritos do usuário", ex);
            }
        }

        public async Task<bool> IsFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _favoriteRepo.ExistsAsync(userId, recipeId, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao verificar favorito", ex);
            }
        }

        public async Task<bool> AddFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar se a receita existe
                var recipe = await _recipeRepo.GetByIdAsync(recipeId);
                if (recipe == null) return false;

                // Verificar se já não é favorito
                var existingFavorite = await _favoriteRepo.ExistsAsync(userId, recipeId, cancellationToken);
                if (existingFavorite) return false; // Já é favorito

                var favorite = new Favorite
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _favoriteRepo.AddAsync(favorite, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao adicionar favorito", ex);
            }
        }

        public async Task<bool> RemoveFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var favorite = await _favoriteRepo.GetFavoriteAsync(userId, recipeId, cancellationToken);
                if (favorite == null) return false;

                await _favoriteRepo.RemoveAsync(favorite, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao remover favorito", ex);
            }
        }

        public async Task<int> GetFavoriteCountAsync(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _favoriteRepo.CountByRecipeAsync(recipeId, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao contar favoritos da receita", ex);
            }
        }

        public async Task<int> GetUserFavoriteCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _favoriteRepo.CountByUserAsync(userId, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao contar favoritos do usuário", ex);
            }
        }

        // Helper methods
        private static FavoriteViewModel ToViewModel(Favorite favorite) => new()
        {
            UserId = favorite.UserId,
            RecipeId = favorite.RecipeId,
            FavoritedAt = favorite.CreatedOnUtc,
            Recipe = favorite.Recipe != null ? new RecipeListViewModel
            {
                Id = favorite.Recipe.Id,
                Title = favorite.Recipe.Title,
                Slug = favorite.Recipe.Slug,
                ImageId = favorite.Recipe.ImageId,
                ImageUrl = favorite.Recipe.ImageId.HasValue ? $"/api/Images/{favorite.Recipe.ImageId}" : null,
                IsPublished = favorite.Recipe.IsPublished,
                PrepTime = favorite.Recipe.PrepTime,
                Servings = favorite.Recipe.Servings,
                Difficulty = favorite.Recipe.Difficulty,
                UserName = favorite.Recipe.User?.FullName,
                CategoryName = favorite.Recipe.Category?.Name
            } : null
        };
    }
}
