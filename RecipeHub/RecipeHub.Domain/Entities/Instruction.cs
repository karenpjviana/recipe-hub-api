using RecipeHub.Domain.Common;

namespace RecipeHub.Domain.Entities;

public class Instruction : BaseEntity
{
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = default!;

    public int StepNumber { get; set; }
    public string Content { get; set; } = default!;
}
