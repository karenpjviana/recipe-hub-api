using RecipeHub.Domain.Common;

namespace RecipeHub.Domain.Entities;

public class RecipeTag
{
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = default!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = default!;

    public DateTime CreatedOnUtc { get; set; }
}
