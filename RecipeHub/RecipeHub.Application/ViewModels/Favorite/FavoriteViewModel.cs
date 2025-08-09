using RecipeHub.Application.ViewModels.Recipe;

namespace RecipeHub.Application.ViewModels.Favorite
{
    public class FavoriteViewModel
    {
        public Guid UserId { get; set; }
        public Guid RecipeId { get; set; }
        public DateTime FavoritedAt { get; set; }
        
        // Dados da receita
        public RecipeListViewModel? Recipe { get; set; }
    }
}
