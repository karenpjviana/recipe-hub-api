namespace RecipeHub.Application.ViewModels.Review
{
    public class ReviewViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RecipeId { get; set; }
        public int Rating { get; set; } // 1-5 estrelas
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Dados computados
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
    }
}
