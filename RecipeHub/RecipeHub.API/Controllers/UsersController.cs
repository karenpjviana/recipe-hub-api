using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.User;

namespace RecipeHub.API.Controllers;

/// <summary>
/// Controller para gerenciamento de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly IImageUploadService _imageUploadService;
    
    // Extensões permitidas para avatar
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    
    // Tamanho máximo: 5MB para avatar
    private const long MaxFileSize = 5 * 1024 * 1024;

    public UsersController(IUserService service, IImageUploadService imageUploadService)
    {
        _service = service;
        _imageUploadService = imageUploadService;
    }

    // PÚBLICO: Ver perfil por username
    [HttpGet("by-username/{username}")]
    public async Task<ActionResult<UserDetailViewModel>> GetByUsername(string username, CancellationToken cancellationToken)
    {
        var vm = await _service.GetUserByUsernameAsync(username, cancellationToken);
        return vm is null ? NotFound() : Ok(vm);
    }

    // AUTENTICADO: Ver meu próprio perfil
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDetailViewModel>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        
        var vm = await _service.GetUserByIdAsync(userId, cancellationToken);
        return vm is null ? NotFound() : Ok(vm);
    }

    // AUTENTICADO: Atualizar meu próprio perfil (JSON simples)
    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UserUpdateViewModel body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        
        return await _service.UpdateUserAsync(userId, body, cancellationToken) ? NoContent() : NotFound();
    }

    // AUTENTICADO: Atualizar perfil com upload de avatar
    [Authorize]
    [HttpPut("me/with-avatar")]
    public async Task<IActionResult> UpdateMyProfileWithAvatar([FromForm] string userData, [FromForm] IFormFile? avatar, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            // Deserializar dados do usuário
            var user = JsonSerializer.Deserialize<UserUpdateViewModel>(userData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (user == null)
                return BadRequest(new { error = "Dados do usuário inválidos" });

            // Processar upload do avatar se fornecido
            if (avatar != null)
            {
                var avatarValidation = ValidateAvatar(avatar);
                if (avatarValidation != null)
                    return BadRequest(avatarValidation);

                // Fazer upload do avatar
                using var stream = avatar.OpenReadStream();
                var avatarUrl = await _imageUploadService.UploadImageAsync(stream, avatar.FileName, cancellationToken);
                
                // Extrair AvatarImageId da URL retornada (/api/Images/{id})
                if (avatarUrl.Contains("/api/Images/"))
                {
                    var imageIdStr = avatarUrl.Substring(avatarUrl.LastIndexOf('/') + 1);
                    if (Guid.TryParse(imageIdStr, out var imageId))
                    {
                        user.AvatarImageId = imageId;
                    }
                }
            }

            // Atualizar usuário
            return await _service.UpdateUserAsync(userId, user, cancellationToken) ? NoContent() : NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Erro interno ao atualizar perfil" });
        }
    }

    /// <summary>
    /// Validar arquivo de avatar
    /// </summary>
    private object? ValidateAvatar(IFormFile avatar)
    {
        if (avatar.Length == 0)
            return new { error = "Arquivo de avatar vazio" };

        if (avatar.Length > MaxFileSize)
            return new { error = $"Arquivo muito grande. Máximo: {MaxFileSize / (1024 * 1024)}MB" };

        var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return new { error = $"Extensão não permitida. Permitidas: {string.Join(", ", _allowedExtensions)}" };

        return null;
    }

    // Helper para pegar o ID do usuário logado
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
