using RecipeHub.Application.ViewModels.Recipe;

namespace RecipeHub.Application.ViewModels.User
{
    public class UserDetailViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid? AvatarImageId { get; set; }
        public string? AvatarUrl { get; set; } // URL computada: /api/Images/{AvatarImageId}
        public int RecipesCount { get; set; }
        public int FavoritesCount { get; set; }
        public int ReviewsCount { get; set; }
        public List<RecipeListViewModel> RecentRecipes { get; set; } = new();
    }
}
