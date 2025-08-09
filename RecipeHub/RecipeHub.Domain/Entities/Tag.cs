using RecipeHub.Domain.Common;

namespace RecipeHub.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = default!;
    public ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();
}
