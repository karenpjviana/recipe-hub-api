using RecipeHub.Application.ViewModels.Instruction;
using RecipeHub.Application.ViewModels.Ingredient;
using RecipeHub.Application.ViewModels.Tag;

namespace RecipeHub.Application.ViewModels.Recipe
{
    public class RecipeCreateViewModel
    {
        public Guid UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? PrepTime { get; set; }
        public int? Servings { get; set; }
        public string? Difficulty { get; set; }
        public Guid? ImageId { get; set; }
        public bool IsPublished { get; set; }

        public List<InstructionCreateViewModel>? Instructions { get; set; }
        public List<IngredientCreateViewModel>? Ingredients { get; set; }
        public List<string>? TagNames { get; set; }
    }
}
