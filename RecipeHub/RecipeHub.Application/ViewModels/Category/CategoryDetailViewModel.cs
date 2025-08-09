using RecipeHub.Application.ViewModels.Recipe;

namespace RecipeHub.Application.ViewModels.Category
{
    public class CategoryDetailViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int RecipesCount { get; set; }
        public List<RecipeListViewModel> RecentRecipes { get; set; } = new();
    }
}
