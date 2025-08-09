using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Category;
using RecipeHub.Application.ViewModels.Recipe;
using RecipeHub.Domain.Entities;
using RecipeHub.Domain.Interfaces;

namespace RecipeHub.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepositoryBase<Category> _categoryRepo;
    private readonly IUnitOfWork _uow;

    public CategoryService(IRepositoryBase<Category> categoryRepo, IUnitOfWork uow)
    {
        _categoryRepo = categoryRepo;
        _uow = uow;
    }

    public async Task<IEnumerable<CategoryListViewModel>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _categoryRepo.GetAllWithIncludesAsync("Recipes");
            return categories.Select(ToListVm).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar categorias", ex);
        }
    }

    public async Task<PaginationResult<CategoryListViewModel>> GetCategoriesPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedCategories = await _categoryRepo.GetPagedWithIncludesAsync(request, "Recipes");
            var viewModels = pagedCategories.Items.Select(ToListVm).ToList();
            
            return new PaginationResult<CategoryListViewModel>(
                viewModels, 
                pagedCategories.TotalItems, 
                pagedCategories.PageNumber, 
                pagedCategories.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar categorias paginadas", ex);
        }
    }

    public async Task<CategoryDetailViewModel?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _categoryRepo.GetByIdWithIncludesAsync(id, "Recipes");
            return entity is null ? null : ToDetailVm(entity);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar categoria com ID {id}", ex);
        }
    }

    public async Task<CategoryDetailViewModel?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _categoryRepo.FirstOrDefaultAsync(c => c.Name.ToLower().Contains(name.ToLower()));
            if (entity == null) return null;
            
            var fullEntity = await _categoryRepo.GetByIdWithIncludesAsync(entity.Id, "Recipes");
            return fullEntity is null ? null : ToDetailVm(fullEntity);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar categoria com nome {name}", ex);
        }
    }

    public async Task<Guid> AddCategoryAsync(CategoryCreateViewModel vm, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new Category
            {
                Name = vm.Name,
                Description = vm.Description
            };

            await _categoryRepo.AddAsync(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao criar categoria", ex);
        }
    }

    public async Task<bool> UpdateCategoryAsync(Guid id, CategoryUpdateViewModel vm, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _categoryRepo.GetByIdAsync(id);
            if (entity is null) return false;

            entity.Name = vm.Name;
            entity.Description = vm.Description;

            _categoryRepo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar categoria com ID {id}", ex);
        }
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _categoryRepo.GetByIdAsync(id);
            if (entity is null) return false;

            _categoryRepo.SoftDelete(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao deletar categoria com ID {id}", ex);
        }
    }

    public async Task<int> GetTotalCategoriesCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _categoryRepo.CountAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao contar categorias", ex);
        }
    }

    // ----------------- Helpers -----------------

    private static CategoryListViewModel ToListVm(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        RecipesCount = c.Recipes?.Count ?? 0
    };

    private static CategoryDetailViewModel ToDetailVm(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        RecipesCount = c.Recipes?.Count ?? 0,
        RecentRecipes = c.Recipes?.Take(5).Select(r => new RecipeListViewModel
        {
            Id = r.Id,
            Title = r.Title,
            Slug = r.Slug,
            ImageId = r.ImageId,
            ImageUrl = r.ImageId.HasValue ? $"/api/Images/{r.ImageId}" : null,
            IsPublished = r.IsPublished
        }).ToList() ?? new()
    };
}
