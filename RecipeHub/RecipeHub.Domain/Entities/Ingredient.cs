using RecipeHub.Domain.Common;

namespace RecipeHub.Domain.Entities;

public class Ingredient : BaseEntity
{
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Unit { get; set; } = default!;
    public decimal Amount { get; set; }
}
