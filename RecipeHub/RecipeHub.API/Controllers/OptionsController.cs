using Microsoft.AspNetCore.Mvc;
using RecipeHub.Domain.Enums;

namespace RecipeHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OptionsController : ControllerBase
{
    [HttpGet("difficulties")]
    public ActionResult<object> GetDifficulties()
    {
        var difficulties = Enum.GetValues<RecipeDifficulty>()
            .Select(d => new
            {
                Value = (int)d,
                Name = d.ToString(),
                DisplayName = GetDifficultyDisplayName(d)
            })
            .ToList();

        return Ok(difficulties);
    }

    private static string GetDifficultyDisplayName(RecipeDifficulty difficulty) => difficulty switch
    {
        RecipeDifficulty.Easy => "Fácil",
        RecipeDifficulty.Medium => "Médio",
        RecipeDifficulty.Hard => "Difícil",
        RecipeDifficulty.Expert => "Expert",
        _ => difficulty.ToString()
    };
}
