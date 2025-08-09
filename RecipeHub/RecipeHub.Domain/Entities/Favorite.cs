using RecipeHub.Domain.Common;

namespace RecipeHub.Domain.Entities;

public class Favorite
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = default!;

    public DateTime CreatedOnUtc { get; set; }
}
