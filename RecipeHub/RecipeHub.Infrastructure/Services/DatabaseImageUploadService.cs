using RecipeHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using RecipeHub.Domain.Interfaces;
using RecipeHub.Domain.Entities;

namespace RecipeHub.Infrastructure.Services;

/// <summary>
/// Serviço de upload que armazena imagens como bytes no banco de dados
/// </summary>
public class DatabaseImageUploadService : IImageUploadService
{
    private readonly ILogger<DatabaseImageUploadService> _logger;
    private readonly IRepositoryBase<ImageStorage> _imageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _baseUrl;

    public DatabaseImageUploadService(
        ILogger<DatabaseImageUploadService> logger,
        IRepositoryBase<ImageStorage> imageRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _logger = logger;
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
        _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5134";
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Converter stream para bytes
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream, cancellationToken);
            var imageBytes = memoryStream.ToArray();

            return await UploadImageAsync(imageBytes, fileName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload da imagem via stream: {FileName}", fileName);
            throw new InvalidOperationException("Falha no upload da imagem", ex);
        }
    }

    public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar entrada
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new ArgumentException("Array de bytes da imagem não pode ser nulo ou vazio", nameof(imageBytes));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Nome do arquivo não pode ser nulo ou vazio", nameof(fileName));
            }

            // Redimensionar imagem para economizar espaço
            var resizedBytes = ResizeImage(imageBytes, 800, 600);
            
            // Determinar tipo de conteúdo baseado na extensão
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? ".jpg";
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
            
            // Gerar ID único para a imagem
            var imageId = Guid.NewGuid();
            
            _logger.LogInformation("Salvando imagem no banco: {ImageId}, Tamanho: {Size} bytes", 
                imageId, resizedBytes.Length);

            // Criar registro no banco
            var imageStorage = new ImageStorage
            {
                Id = imageId,
                FileName = fileName,
                ContentType = contentType,
                Data = resizedBytes,
                Size = resizedBytes.Length
            };

            await _imageRepository.AddAsync(imageStorage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Retornar URL para acessar a imagem via API
            var publicUrl = $"{_baseUrl}/api/Images/{imageId}";
            
            _logger.LogInformation("Upload no banco realizado com sucesso: {Url}", publicUrl);
            return publicUrl;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException || ex is ArgumentException))
        {
            _logger.LogError(ex, "Erro inesperado no upload da imagem no banco: {FileName}", fileName);
            throw new InvalidOperationException("Falha no upload da imagem", ex);
        }
    }

    public byte[] ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight)
    {
        try
        {
            using var image = Image.Load(imageBytes);
            
            // Calcular proporções mantendo aspect ratio
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            // Redimensionar apenas se necessário
            if (newWidth < image.Width || newHeight < image.Height)
            {
                image.Mutate(x => x.Resize(newWidth, newHeight));
            }

            // Converter para bytes (JPEG com qualidade 85% para economizar espaço)
            using var outputStream = new MemoryStream();
            image.Save(outputStream, new JpegEncoder { Quality = 85 });
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao redimensionar imagem");
            return imageBytes; // Retorna original se falhar
        }
    }
}
