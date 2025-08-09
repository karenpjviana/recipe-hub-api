namespace RecipeHub.Application.ViewModels.Ingredient
{
    public class IngredientCreateViewModel
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
