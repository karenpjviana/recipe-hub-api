using System.ComponentModel.DataAnnotations;

namespace RecipeHub.Application.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username é obrigatório")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; } = string.Empty;
    }
}
