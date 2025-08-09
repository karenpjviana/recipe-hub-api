using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Review;

namespace RecipeHub.API.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de reviews de receitas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Listar reviews de uma receita
        /// </summary>
        [HttpGet("recipe/{recipeId:guid}")]
        public async Task<ActionResult<PaginationResult<ReviewViewModel>>> GetReviewsByRecipe(
            Guid recipeId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _reviewService.GetReviewsByRecipeAsync(recipeId, pageNumber, pageSize, cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao buscar reviews" });
            }
        }

        /// <summary>
        /// 👤 Listar reviews de um usuário
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<PaginationResult<ReviewViewModel>>> GetReviewsByUser(
            Guid userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _reviewService.GetReviewsByUserAsync(userId, pageNumber, pageSize, cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao buscar reviews do usuário" });
            }
        }

        /// <summary>
        /// Buscar review específico
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ReviewViewModel>> GetReview(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id, cancellationToken);
                return review == null ? NotFound() : Ok(review);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao buscar review" });
            }
        }

        /// <summary>
        /// Buscar review do usuário para uma receita específica
        /// </summary>
        [HttpGet("user/{userId:guid}/recipe/{recipeId:guid}")]
        public async Task<ActionResult<ReviewViewModel>> GetUserReviewForRecipe(
            Guid userId, 
            Guid recipeId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var review = await _reviewService.GetUserReviewForRecipeAsync(userId, recipeId, cancellationToken);
                return review == null ? NotFound() : Ok(review);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao buscar review" });
            }
        }

        /// <summary>
        /// Meu review para uma receita (usuário logado)
        /// </summary>
        [HttpGet("me/recipe/{recipeId:guid}")]
        [Authorize]
        public async Task<ActionResult<ReviewViewModel>> GetMyReviewForRecipe(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized();

                var review = await _reviewService.GetUserReviewForRecipeAsync(userId, recipeId, cancellationToken);
                return review == null ? NotFound() : Ok(review);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao buscar review" });
            }
        }

        /// <summary>
        /// Criar novo review
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Guid>> CreateReview([FromBody] ReviewCreateViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid) return ValidationProblem(ModelState);

                model.UserId = GetCurrentUserId();
                if (model.UserId == Guid.Empty) return Unauthorized();

                var reviewId = await _reviewService.CreateReviewAsync(model, cancellationToken);
                return CreatedAtAction(nameof(GetReview), new { id = reviewId }, reviewId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao criar review" });
            }
        }

        /// <summary>
        /// Atualizar meu review
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(Guid id, [FromBody] ReviewUpdateViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid) return ValidationProblem(ModelState);

                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized();

                var success = await _reviewService.UpdateReviewAsync(id, userId, model, cancellationToken);
                return success ? NoContent() : NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao atualizar review" });
            }
        }

        /// <summary>
        /// Deletar meu review
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized();

                var success = await _reviewService.DeleteReviewAsync(id, userId, cancellationToken);
                return success ? NoContent() : NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao deletar review" });
            }
        }

        /// <summary>
        /// Estatísticas de reviews de uma receita
        /// </summary>
        [HttpGet("recipe/{recipeId:guid}/stats")]
        public async Task<ActionResult> GetRecipeReviewStats(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var averageRating = await _reviewService.GetAverageRatingAsync(recipeId, cancellationToken);
                var reviewCount = await _reviewService.GetReviewCountAsync(recipeId, cancellationToken);

                return Ok(new
                {
                    recipeId,
                    averageRating = Math.Round(averageRating, 1),
                    reviewCount,
                    ratingDistribution = new // Pode ser implementado depois se necessário
                    {
                        star5 = 0,
                        star4 = 0,
                        star3 = 0,
                        star2 = 0,
                        star1 = 0
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao buscar estatísticas" });
            }
        }

        // Helper para pegar o ID do usuário logado
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
