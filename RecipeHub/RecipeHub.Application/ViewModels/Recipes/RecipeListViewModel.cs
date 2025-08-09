using RecipeHub.Domain.Enums;

namespace RecipeHub.Application.ViewModels.Recipe
{
    public class RecipeListViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public Guid? ImageId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublished { get; set; }
        public int? PrepTime { get; set; }
        public int? Servings { get; set; }
        public string? Difficulty { get; set; }
        public string? UserName { get; set; }
        public string? CategoryName { get; set; }
    }
}
