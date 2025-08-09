using RecipeHub.Domain.Common;
using RecipeHub.Application.ViewModels.Review;

namespace RecipeHub.Application.Interfaces
{
    public interface IReviewService
    {
        Task<PaginationResult<ReviewViewModel>> GetReviewsByRecipeAsync(Guid recipeId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<PaginationResult<ReviewViewModel>> GetReviewsByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<ReviewViewModel?> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ReviewViewModel?> GetUserReviewForRecipeAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task<Guid> CreateReviewAsync(ReviewCreateViewModel vm, CancellationToken cancellationToken = default);
        Task<bool> UpdateReviewAsync(Guid id, Guid userId, ReviewUpdateViewModel vm, CancellationToken cancellationToken = default);
        Task<bool> DeleteReviewAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<double> GetAverageRatingAsync(Guid recipeId, CancellationToken cancellationToken = default);
        Task<int> GetReviewCountAsync(Guid recipeId, CancellationToken cancellationToken = default);
    }
}
