using RecipeHub.Domain.Common;

namespace RecipeHub.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Icon { get; set; }

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
