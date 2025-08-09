using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RecipeHub.Domain.Common;

namespace RecipeHub.Domain.Interfaces
{
    public interface IRepositoryBase<T> where T : BaseEntity
    {
        // Métodos básicos
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        
        // Métodos com includes
        Task<T?> GetByIdWithIncludesAsync(Guid id, params string[] includes);
        Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includes);
        
        // Métodos de busca
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        
        // Métodos de verificação
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        
        // Métodos de contagem
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        
        // Métodos de paginação
        Task<PaginationResult<T>> GetPagedAsync(PaginationRequest request);
        Task<PaginationResult<T>> GetPagedAsync(PaginationRequest request, Expression<Func<T, bool>> predicate);
        Task<PaginationResult<T>> GetPagedWithIncludesAsync(PaginationRequest request, params string[] includes);
        Task<PaginationResult<T>> GetPagedWithIncludesAsync(PaginationRequest request, Expression<Func<T, bool>> predicate, params string[] includes);
        
        // Métodos em lote
        Task AddRangeAsync(IEnumerable<T> entities);
        void UpdateRange(IEnumerable<T> entities);
        void DeleteRange(IEnumerable<T> entities);
        
        // Métodos de soft delete
        void SoftDelete(T entity);
        void SoftDelete(Guid id);
    }
}
