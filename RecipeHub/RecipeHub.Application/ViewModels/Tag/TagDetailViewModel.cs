using RecipeHub.Application.ViewModels.Recipe;

namespace RecipeHub.Application.ViewModels.Tag
{
    public class TagDetailViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int RecipesCount { get; set; }
        public List<RecipeListViewModel> RecentRecipes { get; set; } = new();
    }
}
