using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeHub.Domain.Entities;
using RecipeHub.Domain.Interfaces;
using RecipeHub.Infrastructure.Data;

namespace RecipeHub.Infrastructure.Repositories
{
    public class RecipeRepository : RepositoryBase<Recipe>, IRecipeRepository
    {
        public RecipeRepository(RecipeDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Recipe>> GetPublishedRecipesAsync()
        {
            return await _dbSet.Where(r => r.IsPublished && !r.IsDeleted).ToListAsync();
        }
    }
}