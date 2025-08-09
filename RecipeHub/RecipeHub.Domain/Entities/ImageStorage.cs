using RecipeHub.Domain.Common;

namespace RecipeHub.Domain.Entities;

/// <summary>
/// Entidade para armazenar imagens como bytes no banco de dados
/// </summary>
public class ImageStorage : BaseEntity
{
    /// <summary>
    /// Nome original do arquivo
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de conteúdo MIME (image/jpeg, image/png, etc.)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Dados binários da imagem
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Tamanho do arquivo em bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// ID do usuário que fez o upload
    /// </summary>
    public Guid? UploadedByUserId { get; set; }

    /// <summary>
    /// Usuário que fez o upload
    /// </summary>
    public User? UploadedByUser { get; set; }
}
