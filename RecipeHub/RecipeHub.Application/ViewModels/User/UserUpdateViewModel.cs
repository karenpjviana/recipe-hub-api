namespace RecipeHub.Application.ViewModels.User
{
    public class UserUpdateViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid? AvatarImageId { get; set; }
    }
}
