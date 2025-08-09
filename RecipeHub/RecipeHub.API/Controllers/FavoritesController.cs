using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Favorite;

namespace RecipeHub.API.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de favoritos de receitas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        /// <summary>
        /// Meus favoritos
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<PaginationResult<FavoriteViewModel>>> GetMyFavorites(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized();

                var result = await _favoriteService.GetUserFavoritesAsync(userId, pageNumber, pageSize, cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao buscar favoritos" });
            }
        }

        /// <summary>
        /// 👤 Favoritos de um usuário específico
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        [AllowAnonymous] // Perfil público pode ser visto por todos
        public async Task<ActionResult<PaginationResult<FavoriteViewModel>>> GetUserFavorites(
            Guid userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _favoriteService.GetUserFavoritesAsync(userId, pageNumber, pageSize, cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao buscar favoritos do usuário" });
            }
        }

        /// <summary>
        /// Verificar se receita é favorita
        /// </summary>
        [HttpGet("recipe/{recipeId:guid}/is-favorite")]
        public async Task<ActionResult<bool>> IsRecipeFavorite(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized();

                var isFavorite = await _favoriteService.IsFavoriteAsync(userId, recipeId, cancellationToken);
                return Ok(new { isFavorite });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao verificar favorito" });
            }
        }

        /// <summary>
        /// Adicionar receita aos favoritos
        /// </summary>
        [HttpPost("recipe/{recipeId:guid}")]
        public async Task<IActionResult> AddFavorite(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized();

                var success = await _favoriteService.AddFavoriteAsync(userId, recipeId, cancellationToken);
                
                if (!success)
                    return BadRequest(new { error = "Receita não encontrada ou já é favorita" });

                return Ok(new { message = "Receita adicionada aos favoritos" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao adicionar favorito" });
            }
        }

        /// <summary>
        /// ➖ Remover receita dos favoritos
        /// </summary>
        [HttpDelete("recipe/{recipeId:guid}")]
        public async Task<IActionResult> RemoveFavorite(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized();

                var success = await _favoriteService.RemoveFavoriteAsync(userId, recipeId, cancellationToken);
                
                if (!success)
                    return NotFound(new { error = "Favorito não encontrado" });

                return Ok(new { message = "Receita removida dos favoritos" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao remover favorito" });
            }
        }

        /// <summary>
        /// Toggle favorito (adicionar/remover)
        /// </summary>
        [HttpPost("recipe/{recipeId:guid}/toggle")]
        public async Task<IActionResult> ToggleFavorite(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized();

                var isFavorite = await _favoriteService.IsFavoriteAsync(userId, recipeId, cancellationToken);
                
                bool success;
                string message;

                if (isFavorite)
                {
                    success = await _favoriteService.RemoveFavoriteAsync(userId, recipeId, cancellationToken);
                    message = "Receita removida dos favoritos";
                }
                else
                {
                    success = await _favoriteService.AddFavoriteAsync(userId, recipeId, cancellationToken);
                    message = "Receita adicionada aos favoritos";
                }

                if (!success)
                    return BadRequest(new { error = "Erro ao alterar favorito" });

                return Ok(new { 
                    message, 
                    isFavorite = !isFavorite,
                    action = isFavorite ? "removed" : "added"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao alterar favorito" });
            }
        }

        /// <summary>
        /// Estatísticas de favoritos de uma receita
        /// </summary>
        [HttpGet("recipe/{recipeId:guid}/stats")]
        [AllowAnonymous]
        public async Task<ActionResult> GetRecipeFavoriteStats(Guid recipeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var favoriteCount = await _favoriteService.GetFavoriteCountAsync(recipeId, cancellationToken);

                return Ok(new
                {
                    recipeId,
                    favoriteCount
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno ao buscar estatísticas de favoritos" });
            }
        }

        /// <summary>
        /// Minhas estatísticas de favoritos
        /// </summary>
        [HttpGet("me/stats")]
        public async Task<ActionResult> GetMyFavoriteStats(CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized();

                var favoriteCount = await _favoriteService.GetUserFavoriteCountAsync(userId, cancellationToken);

                return Ok(new
                {
                    userId,
                    favoriteCount
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
