using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeHub.Domain.Entities;

namespace RecipeHub.Domain.Interfaces
{
    public interface IRecipeRepository : IRepositoryBase<Recipe>
    {
        Task<IEnumerable<Recipe>> GetPublishedRecipesAsync();
    }

}
