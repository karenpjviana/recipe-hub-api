using System.ComponentModel.DataAnnotations;

namespace RecipeHub.Application.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username deve ter entre 3 e 50 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "FullName é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome completo deve ter entre 2 e 100 caracteres")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password é obrigatório")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConfirmPassword é obrigatório")]
        [Compare("Password", ErrorMessage = "Senhas não conferem")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public Guid? AvatarImageId { get; set; }
    }
}
