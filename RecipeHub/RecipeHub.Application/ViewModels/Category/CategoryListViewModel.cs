namespace RecipeHub.Application.ViewModels.Category
{
    public class CategoryListViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int RecipesCount { get; set; }
    }
}
