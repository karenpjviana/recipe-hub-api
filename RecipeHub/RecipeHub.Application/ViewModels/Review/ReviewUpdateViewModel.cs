using System.ComponentModel.DataAnnotations;

namespace RecipeHub.Application.ViewModels.Review
{
    public class ReviewUpdateViewModel
    {
        [Required(ErrorMessage = "Rating é obrigatório")]
        [Range(1, 5, ErrorMessage = "Rating deve ser entre 1 e 5")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comentário deve ter no máximo 1000 caracteres")]
        public string? Comment { get; set; }
    }
}
