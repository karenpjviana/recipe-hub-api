using Microsoft.EntityFrameworkCore;
using RecipeHub.Domain.Common;
using RecipeHub.Domain.Entities;
using RecipeHub.Domain.Interfaces;
using RecipeHub.Infrastructure.Data;

namespace RecipeHub.Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly RecipeDbContext _context;

        public FavoriteRepository(RecipeDbContext context)
        {
            _context = context;
        }

        public async Task<PaginationResult<Favorite>> GetUserFavoritesPagedAsync(Guid userId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.Favorites
                .Include(f => f.Recipe)
                    .ThenInclude(r => r!.User)
                .Include(f => f.Recipe)
                    .ThenInclude(r => r!.Category)
                .Where(f => f.UserId == userId);

            var totalItems = await query.CountAsync(cancellationToken);

            var favorites = await query
                .OrderByDescending(f => f.CreatedOnUtc)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginationResult<Favorite>(favorites, totalItems, request.PageNumber, request.PageSize);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId, cancellationToken);
        }

        public async Task<Favorite?> GetFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId, cancellationToken);
        }

        public Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
        {
            _context.Favorites.Add(favorite);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Favorite favorite, CancellationToken cancellationToken = default)
        {
            _context.Favorites.Remove(favorite);
            return Task.CompletedTask;
        }

        public async Task<int> CountByRecipeAsync(Guid recipeId, CancellationToken cancellationToken = default)
        {
            return await _context.Favorites
                .CountAsync(f => f.RecipeId == recipeId, cancellationToken);
        }

        public async Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Favorites
                .CountAsync(f => f.UserId == userId, cancellationToken);
        }
    }
}
