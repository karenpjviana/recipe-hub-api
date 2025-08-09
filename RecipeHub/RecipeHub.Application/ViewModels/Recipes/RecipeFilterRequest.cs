namespace RecipeHub.Application.ViewModels.Recipes;

/// <summary>
/// Parâmetros unificados para filtrar e paginar receitas
/// </summary>
public class RecipeFilterRequest
{
    /// <summary>
    /// Número da página (padrão: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Tamanho da página (padrão: 12)
    /// </summary>
    public int PageSize { get; set; } = 12;
    
    /// <summary>
    /// Termo de busca (título, descrição)
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// ID da categoria para filtrar
    /// </summary>
    public Guid? CategoryId { get; set; }
    
    /// <summary>
    /// ID do usuário para filtrar receitas do autor
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Filtrar apenas receitas publicadas
    /// </summary>
    public bool? IsPublished { get; set; }
    
    /// <summary>
    /// Dificuldade da receita
    /// </summary>
    public string? Difficulty { get; set; }
    
    /// <summary>
    /// Tempo máximo de preparo (em minutos)
    /// </summary>
    public int? MaxPrepTime { get; set; }
    
    /// <summary>
    /// Número mínimo de porções
    /// </summary>
    public int? MinServings { get; set; }
    
    /// <summary>
    /// Número máximo de porções
    /// </summary>
    public int? MaxServings { get; set; }
    
    /// <summary>
    /// Tags para filtrar (separadas por vírgula)
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// Ordenação: title, prepTime, servings, createdAt (padrão: createdAt)
    /// </summary>
    public string? SortBy { get; set; } = "createdAt";
    
    /// <summary>
    /// Direção da ordenação: asc, desc (padrão: desc)
    /// </summary>
    public string? SortDirection { get; set; } = "desc";
}
