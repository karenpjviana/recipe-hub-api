namespace RecipeHub.Application.ViewModels.User
{
    public class UserListViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid? AvatarImageId { get; set; }
        public string? AvatarUrl { get; set; } // URL computada: /api/Images/{AvatarImageId}
        public int RecipesCount { get; set; }
    }
}
