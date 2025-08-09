using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Review;
using RecipeHub.Domain.Entities;
using RecipeHub.Domain.Interfaces;

namespace RecipeHub.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRepositoryBase<Review> _reviewRepo;
        private readonly IRepositoryBase<Recipe> _recipeRepo;
        private readonly IRepositoryBase<User> _userRepo;
        private readonly IUnitOfWork _uow;

        public ReviewService(
            IRepositoryBase<Review> reviewRepo, 
            IRepositoryBase<Recipe> recipeRepo,
            IRepositoryBase<User> userRepo,
            IUnitOfWork uow)
        {
            _reviewRepo = reviewRepo;
            _recipeRepo = recipeRepo;
            _userRepo = userRepo;
            _uow = uow;
        }

        public async Task<PaginationResult<ReviewViewModel>> GetReviewsByRecipeAsync(Guid recipeId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize };
                
                var pagedReviews = await _reviewRepo.GetPagedWithIncludesAsync(
                    request, 
                    r => r.RecipeId == recipeId,
                    "User"
                );

                var viewModels = pagedReviews.Items.Select(ToViewModel).ToList();
                
                return new PaginationResult<ReviewViewModel>(
                    viewModels, 
                    pagedReviews.TotalItems, 
                    pagedReviews.PageNumber, 
                    pagedReviews.PageSize
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao buscar reviews da receita", ex);
            }
        }

        public async Task<PaginationResult<ReviewViewModel>> GetReviewsByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize };
                
                var pagedReviews = await _reviewRepo.GetPagedWithIncludesAsync(
                    request, 
                    r => r.UserId == userId,
                    "User", "Recipe"
                );

                var viewModels = pagedReviews.Items.Select(ToViewModel).ToList();
                
                return new PaginationResult<ReviewViewModel>(
                    viewModels, 
                    pagedReviews.TotalItems, 
                    pagedReviews.PageNumber, 
                    pagedReviews.PageSize
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao buscar reviews do usuário", ex);
            }
        }

        public async Task<ReviewViewModel?> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var review = await _reviewRepo.GetByIdWithIncludesAsync(id, "User");
                return review == null ? null : ToViewModel(review);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao buscar review", ex);
            }
        }

        public async Task<ReviewViewModel?> GetUserReviewForRecipeAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Como precisamos do User, vamos buscar todos e filtrar
                var reviews = await _reviewRepo.FindAsync(r => r.UserId == userId && r.RecipeId == recipeId);
                var review = reviews.FirstOrDefault();
                
                // Se encontrou o review, buscar novamente com includes para ter o User
                if (review != null)
                {
                    review = await _reviewRepo.GetByIdWithIncludesAsync(review.Id, "User");
                }
                
                return review == null ? null : ToViewModel(review);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao buscar review do usuário para a receita", ex);
            }
        }

        public async Task<Guid> CreateReviewAsync(ReviewCreateViewModel vm, CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar se a receita existe
                var recipe = await _recipeRepo.GetByIdAsync(vm.RecipeId);
                if (recipe == null)
                    throw new InvalidOperationException("Receita não encontrada");

                // Verificar se o usuário já avaliou esta receita
                var existingReview = await _reviewRepo.FirstOrDefaultAsync(r => r.UserId == vm.UserId && r.RecipeId == vm.RecipeId);
                if (existingReview != null)
                    throw new InvalidOperationException("Usuário já avaliou esta receita");

                var review = new Review
                {
                    UserId = vm.UserId!.Value,
                    RecipeId = vm.RecipeId,
                    Rating = vm.Rating,
                    Comment = vm.Comment
                };

                await _reviewRepo.AddAsync(review);
                await _uow.SaveChangesAsync(cancellationToken);

                return review.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao criar review", ex);
            }
        }

        public async Task<bool> UpdateReviewAsync(Guid id, Guid userId, ReviewUpdateViewModel vm, CancellationToken cancellationToken = default)
        {
            try
            {
                var review = await _reviewRepo.GetByIdAsync(id);
                if (review == null || review.UserId != userId) return false;

                review.Rating = vm.Rating;
                review.Comment = vm.Comment;

                _reviewRepo.Update(review);
                await _uow.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao atualizar review", ex);
            }
        }

        public async Task<bool> DeleteReviewAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var review = await _reviewRepo.GetByIdAsync(id);
                if (review == null || review.UserId != userId) return false;

                _reviewRepo.SoftDelete(review);
                await _uow.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao deletar review", ex);
            }
        }

        public async Task<double> GetAverageRatingAsync(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var reviews = await _reviewRepo.FindAsync(r => r.RecipeId == recipeId);
                if (!reviews.Any()) return 0;

                return reviews.Average(r => r.Rating);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao calcular rating médio", ex);
            }
        }

        public async Task<int> GetReviewCountAsync(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _reviewRepo.CountAsync(r => r.RecipeId == recipeId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao contar reviews", ex);
            }
        }

        // Helper methods
        private static ReviewViewModel ToViewModel(Review review) => new()
        {
            Id = review.Id,
            UserId = review.UserId,
            RecipeId = review.RecipeId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedOnUtc,
            UserName = review.User?.FullName ?? "Usuário",
            UserAvatarUrl = review.User?.AvatarImageId.HasValue == true 
                ? $"/api/Images/{review.User.AvatarImageId}" 
                : null
        };
    }
}
