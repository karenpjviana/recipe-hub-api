namespace RecipeHub.Domain.Common;

/// <summary>
/// Parâmetros de filtro para busca de usuários
/// </summary>
public class UserFilterParameters : PaginationRequest
{
    /// <summary>
    /// Busca por nome ou username (case insensitive)
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// Filtrar por role (User, Admin)
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// Filtrar usuários que têm receitas publicadas
    /// </summary>
    public bool? HasPublishedRecipes { get; set; }
    
    /// <summary>
    /// Ordenação: CreatedAt, FullName, Username, RecipeCount
    /// </summary>
    public string OrderBy { get; set; } = "CreatedAt";
    
    /// <summary>
    /// Direção da ordenação: asc ou desc
    /// </summary>
    public string OrderDirection { get; set; } = "desc";
}
