namespace RecipeHub.Application.ViewModels.Tag
{
    public class TagListViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int RecipesCount { get; set; }
    }
}
