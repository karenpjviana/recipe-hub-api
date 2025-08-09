using RecipeHub.Domain.Common;

namespace RecipeHub.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public Guid? AvatarImageId { get; set; }
    public ImageStorage? AvatarImage { get; set; }
    public string Role { get; set; } = "User"; // Default: User, Admin

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
