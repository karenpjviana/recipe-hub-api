using RecipeHub.Domain.Interfaces;

namespace RecipeHub.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RecipeDbContext _context;

        public UnitOfWork(RecipeDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}