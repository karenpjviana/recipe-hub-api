using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Recipe;
using RecipeHub.Application.ViewModels.Recipes;

namespace RecipeHub.API.Controllers;

/// <summary>
/// Controller principal para gerenciamento de receitas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _service;
    private readonly IImageUploadService _imageUploadService;
    
    // Extensões permitidas
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    
    // Tamanho máximo: 10MB
    private const long MaxFileSize = 10 * 1024 * 1024;

    public RecipesController(IRecipeService service, IImageUploadService imageUploadService)
    {
        _service = service;
        _imageUploadService = imageUploadService;
    }

    /// <summary>
    /// 🎯 ENDPOINT UNIFICADO - Substitui 6 endpoints antigos!
    /// Suporta busca, filtros e paginação em uma única requisição
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResult<RecipeListViewModel>>> GetRecipes(
        [FromQuery] RecipeFilterRequest filter, 
        CancellationToken cancellationToken = default)
        => Ok(await _service.GetRecipesAsync(filter, cancellationToken));

    /// <summary>
    /// 📄 Buscar receita por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RecipeDetailViewModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vm = await _service.GetRecipeByIdAsync(id, cancellationToken);
        return vm is null ? NotFound() : Ok(vm);
    }

    /// <summary>
    /// 🌐 Buscar receita por SLUG (URL amigável)
    /// Exemplo: /recipes/pao-de-acucar-delicioso
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<RecipeDetailViewModel>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var vm = await _service.GetRecipeBySlugAsync(slug, cancellationToken);
        return vm is null ? NotFound() : Ok(vm);
    }

    /// <summary>
    /// Criar nova receita (JSON simples, sem imagem)
    /// </summary>
    [HttpPost("json")]
    [Authorize]
    public async Task<ActionResult<Guid>> CreateJson([FromBody] RecipeCreateViewModel body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        // Definir usuário atual
        body.UserId = GetCurrentUserId();
        
        var id = await _service.AddRecipeAsync(body, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Criar nova receita (com upload de imagem integrado)
    /// </summary>
    [HttpPost("with-image")]
    [Authorize]
    public async Task<ActionResult<Guid>> CreateWithImage([FromForm] string recipeData, [FromForm] IFormFile? image, CancellationToken cancellationToken)
    {
        try
        {
            // Deserializar dados da receita
            var recipe = JsonSerializer.Deserialize<RecipeCreateViewModel>(recipeData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (recipe == null)
                return BadRequest(new { error = "Dados da receita inválidos" });

            // Definir usuário atual
            recipe.UserId = GetCurrentUserId();

            // Processar upload da imagem se fornecida
            if (image != null)
            {
                var imageValidation = ValidateImage(image);
                if (imageValidation != null)
                    return BadRequest(imageValidation);

                // Fazer upload da imagem
                using var stream = image.OpenReadStream();
                var imageUrl = await _imageUploadService.UploadImageAsync(stream, image.FileName, cancellationToken);
                
                // Extrair ImageId da URL retornada (/api/Images/{id})
                if (imageUrl.Contains("/api/Images/"))
                {
                    var imageIdStr = imageUrl.Substring(imageUrl.LastIndexOf('/') + 1);
                    if (Guid.TryParse(imageIdStr, out var imageId))
                    {
                        recipe.ImageId = imageId;
                    }
                }
            }

            // Criar receita
            var id = await _service.AddRecipeAsync(recipe, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Erro interno ao criar receita" });
        }
    }

    /// <summary>
    /// Atualizar receita existente (JSON simples, sem imagem)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] RecipeUpdateViewModel body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        return await _service.UpdateRecipeAsync(id, body, cancellationToken) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Atualizar receita existente (com upload de imagem integrado)
    /// </summary>
    [HttpPut("{id:guid}/with-image")]
    [Authorize]
    public async Task<IActionResult> UpdateWithImage(Guid id, [FromForm] string recipeData, [FromForm] IFormFile? image, CancellationToken cancellationToken)
    {
        try
        {
            // Deserializar dados da receita
            var recipe = JsonSerializer.Deserialize<RecipeUpdateViewModel>(recipeData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (recipe == null)
                return BadRequest(new { error = "Dados da receita inválidos" });

            // Processar upload da imagem se fornecida
            if (image != null)
            {
                var imageValidation = ValidateImage(image);
                if (imageValidation != null)
                    return BadRequest(imageValidation);

                // Fazer upload da imagem
                using var stream = image.OpenReadStream();
                var imageUrl = await _imageUploadService.UploadImageAsync(stream, image.FileName, cancellationToken);
                
                // Extrair ImageId da URL retornada (/api/Images/{id})
                if (imageUrl.Contains("/api/Images/"))
                {
                    var imageIdStr = imageUrl.Substring(imageUrl.LastIndexOf('/') + 1);
                    if (Guid.TryParse(imageIdStr, out var imageId))
                    {
                        recipe.ImageId = imageId;
                    }
                }
            }

            // Atualizar receita
            return await _service.UpdateRecipeAsync(id, recipe, cancellationToken) ? NoContent() : NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Erro interno ao atualizar receita" });
        }
    }

    /// <summary>
    /// Deletar receita (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await _service.DeleteRecipeAsync(id, cancellationToken) ? NoContent() : NotFound();

    /// <summary>
    /// Validar arquivo de imagem
    /// </summary>
    private object? ValidateImage(IFormFile image)
    {
        if (image.Length == 0)
            return new { error = "Arquivo de imagem vazio" };

        if (image.Length > MaxFileSize)
            return new { error = $"Arquivo muito grande. Máximo: {MaxFileSize / (1024 * 1024)}MB" };

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return new { error = $"Extensão não permitida. Permitidas: {string.Join(", ", _allowedExtensions)}" };

        return null;
    }

    /// <summary>
    /// Extrair ID do usuário atual do JWT
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}