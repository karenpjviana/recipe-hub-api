namespace RecipeHub.Application.Interfaces;

/// <summary>
/// Serviço para upload de imagens na nuvem
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Faz upload de uma imagem e retorna a URL
    /// </summary>
    /// <param name="imageStream">Stream da imagem</param>
    /// <param name="fileName">Nome do arquivo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>URL da imagem hospedada</returns>
    Task<string> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Faz upload de uma imagem a partir de bytes
    /// </summary>
    /// <param name="imageBytes">Bytes da imagem</param>
    /// <param name="fileName">Nome do arquivo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>URL da imagem hospedada</returns>
    Task<string> UploadImageAsync(byte[] imageBytes, string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Redimensiona imagem para tamanhos específicos
    /// </summary>
    /// <param name="imageBytes">Bytes da imagem original</param>
    /// <param name="maxWidth">Largura máxima</param>
    /// <param name="maxHeight">Altura máxima</param>
    /// <returns>Bytes da imagem redimensionada</returns>
    byte[] ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight);
}
