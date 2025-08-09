using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeHub.Domain.Common;
using RecipeHub.Domain.Interfaces;
using RecipeHub.Infrastructure.Data;

namespace RecipeHub.Infrastructure.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : BaseEntity
    {
        protected readonly RecipeDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositoryBase(RecipeDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // Métodos básicos
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity); // Soft delete será aplicado pelo interceptor
        }

        // Métodos com includes
        public async Task<T?> GetByIdWithIncludesAsync(Guid id, params string[] includes)
        {
            IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted);
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includes)
        {
            IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted);
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.ToListAsync();
        }

        // Métodos de busca
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).Where(predicate).ToListAsync();
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).FirstOrDefaultAsync(predicate);
        }

        // Métodos de verificação
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate);
        }

        // Métodos de contagem
        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync(e => !e.IsDeleted);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => !e.IsDeleted).CountAsync(predicate);
        }

        // Métodos de paginação
        public async Task<PaginationResult<T>> GetPagedAsync(PaginationRequest request)
        {
            var query = _dbSet.Where(e => !e.IsDeleted);
            var totalItems = await query.CountAsync();
            
            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            return new PaginationResult<T>(items, totalItems, request.PageNumber, request.PageSize);
        }

        public async Task<PaginationResult<T>> GetPagedAsync(PaginationRequest request, Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.Where(e => !e.IsDeleted).Where(predicate);
            var totalItems = await query.CountAsync();
            
            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            return new PaginationResult<T>(items, totalItems, request.PageNumber, request.PageSize);
        }

        public async Task<PaginationResult<T>> GetPagedWithIncludesAsync(PaginationRequest request, params string[] includes)
        {
            IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted);
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            var totalItems = await query.CountAsync();
            
            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            return new PaginationResult<T>(items, totalItems, request.PageNumber, request.PageSize);
        }

        public async Task<PaginationResult<T>> GetPagedWithIncludesAsync(PaginationRequest request, Expression<Func<T, bool>> predicate, params string[] includes)
        {
            IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted).Where(predicate);
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            var totalItems = await query.CountAsync();
            
            var items = await query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync();

            return new PaginationResult<T>(items, totalItems, request.PageNumber, request.PageSize);
        }

        // Métodos em lote
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        // Métodos de soft delete
        public void SoftDelete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedOnUtc = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public void SoftDelete(Guid id)
        {
            var entity = _dbSet.FirstOrDefault(e => e.Id == id && !e.IsDeleted);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.DeletedOnUtc = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
        }
    }
}