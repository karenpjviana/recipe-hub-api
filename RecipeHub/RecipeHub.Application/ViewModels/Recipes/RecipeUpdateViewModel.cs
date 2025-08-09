using RecipeHub.Application.ViewModels.Instruction;
using RecipeHub.Application.ViewModels.Ingredient;
using RecipeHub.Application.ViewModels.Tag;
using RecipeHub.Domain.Enums;

namespace RecipeHub.Application.ViewModels.Recipe
{
    public class RecipeUpdateViewModel
    {
        public Guid? CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? PrepTime { get; set; }
        public int? Servings { get; set; }
        public string? Difficulty { get; set; }
        public Guid? ImageId { get; set; }
        public bool IsPublished { get; set; }

        public List<InstructionViewModel> Instructions { get; set; } = new();
        public List<IngredientViewModel> Ingredients { get; set; } = new();
        public List<string> TagNames { get; set; } = new();
    }
}
