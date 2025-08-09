using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Tag;
using RecipeHub.Application.ViewModels.Recipe;
using RecipeHub.Domain.Entities;
using RecipeHub.Domain.Interfaces;

namespace RecipeHub.Application.Services;

public class TagService : ITagService
{
    private readonly IRepositoryBase<Tag> _tagRepo;
    private readonly IUnitOfWork _uow;

    public TagService(IRepositoryBase<Tag> tagRepo, IUnitOfWork uow)
    {
        _tagRepo = tagRepo;
        _uow = uow;
    }

    public async Task<IEnumerable<TagListViewModel>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tags = await _tagRepo.GetAllWithIncludesAsync("RecipeTags.Recipe");
            return tags.Select(ToListVm).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar tags", ex);
        }
    }

    public async Task<PaginationResult<TagListViewModel>> GetTagsPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedTags = await _tagRepo.GetPagedWithIncludesAsync(request, "RecipeTags.Recipe");
            var viewModels = pagedTags.Items.Select(ToListVm).ToList();
            
            return new PaginationResult<TagListViewModel>(
                viewModels, 
                pagedTags.TotalItems, 
                pagedTags.PageNumber, 
                pagedTags.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar tags paginadas", ex);
        }
    }

    public async Task<TagDetailViewModel?> GetTagByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _tagRepo.GetByIdWithIncludesAsync(id, "RecipeTags.Recipe");
            return entity is null ? null : ToDetailVm(entity);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar tag com ID {id}", ex);
        }
    }

    public async Task<TagDetailViewModel?> GetTagByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _tagRepo.FirstOrDefaultAsync(t => t.Name.ToLower().Contains(name.ToLower()));
            if (entity == null) return null;
            
            var fullEntity = await _tagRepo.GetByIdWithIncludesAsync(entity.Id, "RecipeTags.Recipe");
            return fullEntity is null ? null : ToDetailVm(fullEntity);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar tag com nome {name}", ex);
        }
    }

    public async Task<Guid> AddTagAsync(TagCreateViewModel vm, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new Tag
            {
                Name = vm.Name
            };

            await _tagRepo.AddAsync(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao criar tag", ex);
        }
    }

    public async Task<bool> UpdateTagAsync(Guid id, TagUpdateViewModel vm, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _tagRepo.GetByIdAsync(id);
            if (entity is null) return false;

            entity.Name = vm.Name;

            _tagRepo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar tag com ID {id}", ex);
        }
    }

    public async Task<bool> DeleteTagAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _tagRepo.GetByIdAsync(id);
            if (entity is null) return false;

            _tagRepo.SoftDelete(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao deletar tag com ID {id}", ex);
        }
    }

    public async Task<int> GetTotalTagsCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _tagRepo.CountAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao contar tags", ex);
        }
    }

    // ----------------- Helpers -----------------

    private static TagListViewModel ToListVm(Tag t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        RecipesCount = t.RecipeTags?.Count ?? 0
    };

    private static TagDetailViewModel ToDetailVm(Tag t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        RecipesCount = t.RecipeTags?.Count ?? 0,
        RecentRecipes = t.RecipeTags?.Take(5).Select(rt => new RecipeListViewModel
        {
            Id = rt.Recipe.Id,
            Title = rt.Recipe.Title,
            Slug = rt.Recipe.Slug,
            ImageId = rt.Recipe.ImageId,
            ImageUrl = rt.Recipe.ImageId.HasValue ? $"/api/Images/{rt.Recipe.ImageId}" : null,
            IsPublished = rt.Recipe.IsPublished
        }).ToList() ?? new()
    };
}
