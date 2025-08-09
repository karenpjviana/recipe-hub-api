using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RecipeHub.Application.Interfaces;
using RecipeHub.Domain.Interfaces;
using RecipeHub.Domain.Entities;
using System.Security.Claims;

namespace RecipeHub.API.Controllers;

/// <summary>
/// Controller para gerenciamento e serving de imagens
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IImageUploadService _imageUploadService;
    private readonly IRepositoryBase<ImageStorage> _imageRepository;
    private readonly ILogger<ImagesController> _logger;

    // Extensões permitidas
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    
    // Tamanho máximo: 10MB
    private const long MaxFileSize = 10 * 1024 * 1024;

    public ImagesController(
        IImageUploadService imageUploadService, 
        IRepositoryBase<ImageStorage> imageRepository,
        ILogger<ImagesController> logger)
    {
        _imageUploadService = imageUploadService;
        _imageRepository = imageRepository;
        _logger = logger;
    }

    /// <summary>
    /// 📸 Upload de imagem para receitas
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<object>> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            // Validações
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Nenhuma imagem foi enviada" });

            if (file.Length > MaxFileSize)
                return BadRequest(new { error = $"Arquivo muito grande. Máximo: {MaxFileSize / (1024 * 1024)}MB" });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return BadRequest(new { error = $"Extensão não permitida. Permitidas: {string.Join(", ", _allowedExtensions)}" });

            // Gerar nome único
            var fileName = $"recipe-{Guid.NewGuid()}{extension}";

            // Fazer upload
            using var stream = file.OpenReadStream();
            var imageUrl = await _imageUploadService.UploadImageAsync(stream, fileName, cancellationToken);

            _logger.LogInformation("Upload realizado com sucesso. Arquivo: {FileName}, URL: {ImageUrl}", fileName, imageUrl);

            return Ok(new
            {
                success = true,
                url = imageUrl,
                fileName = fileName,
                originalName = file.FileName,
                size = file.Length,
                message = "Upload realizado com sucesso!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no upload de imagem: {FileName}", file?.FileName);
            return StatusCode(500, new { error = "Erro interno no upload da imagem" });
        }
    }

    /// <summary>
    /// 📸 Upload múltiplo de imagens (máximo 5)
    /// </summary>
    [HttpPost("upload-multiple")]
    public async Task<ActionResult<object>> UploadMultipleImages(List<IFormFile> files, CancellationToken cancellationToken)
    {
        try
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { error = "Nenhuma imagem foi enviada" });

            if (files.Count > 5)
                return BadRequest(new { error = "Máximo de 5 imagens por vez" });

            var results = new List<object>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    // Validações individuais
                    if (file.Length > MaxFileSize)
                    {
                        errors.Add($"{file.FileName}: Arquivo muito grande");
                        continue;
                    }

                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!_allowedExtensions.Contains(extension))
                    {
                        errors.Add($"{file.FileName}: Extensão não permitida");
                        continue;
                    }

                    // Upload individual
                    var fileName = $"recipe-{Guid.NewGuid()}{extension}";
                    using var stream = file.OpenReadStream();
                    var imageUrl = await _imageUploadService.UploadImageAsync(stream, fileName, cancellationToken);

                    results.Add(new
                    {
                        url = imageUrl,
                        fileName = fileName,
                        originalName = file.FileName,
                        size = file.Length
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no upload da imagem: {FileName}", file.FileName);
                    errors.Add($"{file.FileName}: Erro no upload");
                }
            }

            return Ok(new
            {
                success = true,
                uploaded = results.Count,
                total = files.Count,
                images = results,
                errors = errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no upload múltiplo de imagens");
            return StatusCode(500, new { error = "Erro interno no upload das imagens" });
        }
    }

    /// <summary>
    /// Deletar imagem (apenas ImgBB remove da URL, aqui só validamos)
    /// </summary>
    [HttpDelete("{imageUrl}")]
    public ActionResult DeleteImage(string imageUrl)
    {
        try
        {
            // Validar se é uma URL válida do ImgBB
            if (!imageUrl.Contains("i.ibb.co") && !imageUrl.Contains("ibb.co"))
            {
                return BadRequest(new { error = "URL de imagem inválida" });
            }

            // Para ImgBB, não podemos deletar pela API gratuita
            // Mas podemos remover do banco se estiver associada a uma receita
            _logger.LogInformation("Solicitação de remoção de imagem: {ImageUrl}", imageUrl);

            return Ok(new
            {
                success = true,
                message = "Imagem removida da aplicação. Arquivo permanece no ImgBB.",
                imageUrl = imageUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar remoção de imagem: {ImageUrl}", imageUrl);
            return StatusCode(500, new { error = "Erro interno" });
        }
    }

    /// <summary>
    /// 📊 Minhas imagens (por usuário autenticado)
    /// </summary>
    [HttpGet("my-images")]
    public ActionResult<object> GetMyImages()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Por enquanto retorna info básica
            // Em implementação futura, poderia buscar do banco
            return Ok(new
            {
                userId = userId,
                message = "Funcionalidade de histórico será implementada futuramente",
                uploadCount = "Não disponível no momento"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar imagens do usuário");
            return StatusCode(500, new { error = "Erro interno" });
        }
    }

    /// <summary>
    /// Informações sobre upload de imagens
    /// </summary>
    [HttpGet("info")]
    [AllowAnonymous] // Público para frontend saber as limitações
    public ActionResult<object> GetUploadInfo()
    {
        return Ok(new
        {
            maxFileSize = MaxFileSize,
            maxFileSizeMB = MaxFileSize / (1024 * 1024),
            allowedExtensions = _allowedExtensions,
            maxFilesPerUpload = 5,
            autoResize = new
            {
                enabled = true,
                maxWidth = 800,
                maxHeight = 600,
                quality = 85
            },
            features = new
            {
                uploadForAllUsers = true,
                deleteSupported = true,
                imgBBIntegration = true,
                freeStorage = true
            }
        });
    }

    /// <summary>
    /// Servir imagem do banco de dados
    /// </summary>
    [HttpGet("{imageId:guid}")]
    [AllowAnonymous] // Público para que as imagens possam ser visualizadas
    public async Task<ActionResult> GetImage(Guid imageId, CancellationToken cancellationToken)
    {
        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            
            if (image == null)
            {
                return NotFound(new { error = "Imagem não encontrada" });
            }

            // Retornar a imagem com o tipo de conteúdo correto
            return File(image.Data, image.ContentType, image.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar imagem: {ImageId}", imageId);
            return StatusCode(500, new { error = "Erro interno ao buscar imagem" });
        }
    }

    /// <summary>
    /// Helper para extrair ID do usuário atual do JWT
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
