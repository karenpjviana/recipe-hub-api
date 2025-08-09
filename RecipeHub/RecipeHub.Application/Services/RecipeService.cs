using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Ingredient;
using RecipeHub.Application.ViewModels.Instruction;
using RecipeHub.Application.ViewModels.Recipe;
using RecipeHub.Application.ViewModels.Recipes;
using RecipeHub.Domain.Enums;
using RecipeHub.Domain.Entities;
using RecipeHub.Domain.Interfaces;

public class RecipeService : IRecipeService
{
    private readonly IRepositoryBase<Recipe> _recipeRepo;
    private readonly IRepositoryBase<Tag> _tagRepo;
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _configuration;

    public RecipeService(IRepositoryBase<Recipe> recipeRepo,
                         IRepositoryBase<Tag> tagRepo,
                         IUnitOfWork uow,
                         IConfiguration configuration)
    {
        _recipeRepo = recipeRepo;
        _tagRepo = tagRepo;
        _uow = uow;
        _configuration = configuration;
    }

    // ✅ MÉTODO UNIFICADO - Substitui múltiplos endpoints
    public async Task<PaginationResult<RecipeListViewModel>> GetRecipesAsync(RecipeFilterRequest filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            // Construir expressão de filtro dinâmicamente
            Expression<Func<Recipe, bool>> filterExpression = r => true;

            // Filtro de busca (título ou descrição)
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                filterExpression = CombineExpressions(filterExpression, 
                    r => r.Title.ToLower().Contains(searchLower) || 
                         (r.Description != null && r.Description.ToLower().Contains(searchLower)));
            }

            // Filtro por categoria
            if (filter.CategoryId.HasValue)
            {
                filterExpression = CombineExpressions(filterExpression, r => r.CategoryId == filter.CategoryId.Value);
            }

            // Filtro por usuário
            if (filter.UserId.HasValue)
            {
                filterExpression = CombineExpressions(filterExpression, r => r.UserId == filter.UserId.Value);
            }

            // Filtro de publicação
            if (filter.IsPublished.HasValue)
            {
                filterExpression = CombineExpressions(filterExpression, r => r.IsPublished == filter.IsPublished.Value);
            }

            // Filtro por dificuldade
            if (!string.IsNullOrWhiteSpace(filter.Difficulty))
            {
                filterExpression = CombineExpressions(filterExpression, r => r.Difficulty == filter.Difficulty);
            }

            // Filtro por tempo de preparo
            if (filter.MaxPrepTime.HasValue)
            {
                filterExpression = CombineExpressions(filterExpression, r => r.PrepTime <= filter.MaxPrepTime.Value);
            }

            // Filtro por porções
            if (filter.MinServings.HasValue)
            {
                filterExpression = CombineExpressions(filterExpression, r => r.Servings >= filter.MinServings.Value);
            }

            if (filter.MaxServings.HasValue)
            {
                filterExpression = CombineExpressions(filterExpression, r => r.Servings <= filter.MaxServings.Value);
            }



            // Buscar receitas com filtros aplicados
            var pagedRecipes = await _recipeRepo.GetPagedWithIncludesAsync(request, filterExpression, "User", "Category");
            
            // Aplicar ordenação
            var viewModels = pagedRecipes.Items.Select(ToListVm).ToList();
            
            return new PaginationResult<RecipeListViewModel>(
                viewModels, 
                pagedRecipes.TotalItems, 
                pagedRecipes.PageNumber, 
                pagedRecipes.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar receitas", ex);
        }
    }

    public async Task<PaginationResult<RecipeListViewModel>> GetRecipesPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedRecipes = await _recipeRepo.GetPagedWithIncludesAsync(request, "User", "Category");
            var viewModels = pagedRecipes.Items.Select(ToListVm).ToList();
            
            return new PaginationResult<RecipeListViewModel>(
                viewModels, 
                pagedRecipes.TotalItems, 
                pagedRecipes.PageNumber, 
                pagedRecipes.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar receitas paginadas", ex);
        }
    }

    public async Task<RecipeDetailViewModel?> GetRecipeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _recipeRepo.GetByIdWithIncludesAsync(id, 
                "Instructions", "Ingredients", "RecipeTags.Tag", "User", "Category");
            return entity is null ? null : ToDetailVm(entity);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar receita com ID {id}", ex);
        }
    }

    public async Task<RecipeDetailViewModel?> GetRecipeBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _recipeRepo.FirstOrDefaultAsync(r => r.Slug == slug);
            if (entity == null) return null;
            
            var fullEntity = await _recipeRepo.GetByIdWithIncludesAsync(entity.Id, 
                "Instructions", "Ingredients", "RecipeTags.Tag", "User", "Category");
            return fullEntity is null ? null : ToDetailVm(fullEntity);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar receita com slug {slug}", ex);
        }
    }

    public async Task<Guid> AddRecipeAsync(RecipeCreateViewModel vm, CancellationToken cancellationToken = default)
    {
        try
        {
            var instructions = vm.Instructions?.Select(MapInstructionCreate).ToList() ?? new List<Instruction>();
            var ingredients = vm.Ingredients?.Select(MapIngredientCreate).ToList() ?? new List<Ingredient>();
            var recipeTags = await ProcessTagNamesAsync(vm.TagNames, cancellationToken);

            var entity = Recipe.Create(
                vm.UserId, vm.CategoryId, vm.Title, vm.Description, vm.PrepTime, vm.Servings,
                vm.Difficulty, vm.ImageId, vm.IsPublished,
                instructions, ingredients, recipeTags
            );

            entity.Slug = await GenerateUniqueSlugAsync(entity.Slug);

            await _recipeRepo.AddAsync(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao criar receita", ex);
        }
    }

    public async Task<bool> UpdateRecipeAsync(Guid id, RecipeUpdateViewModel vm, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _recipeRepo.GetByIdWithIncludesAsync(id, 
                "Instructions", "Ingredients", "RecipeTags");
            if (entity is null) return false;

            var instructions = vm.Instructions?.Select(MapInstruction).ToList() ?? new();
            var ingredients = vm.Ingredients?.Select(MapIngredient).ToList() ?? new();
            var recipeTags = await ProcessTagNamesAsync(vm.TagNames, cancellationToken);

            entity.Update(vm.CategoryId, vm.Title, vm.Description, vm.PrepTime, vm.Servings,
                      vm.Difficulty, vm.ImageId, vm.IsPublished,
                      instructions, ingredients, recipeTags);

            entity.Slug = await GenerateUniqueSlugAsync(entity.Slug, currentId: entity.Id);

            _recipeRepo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar receita com ID {id}", ex);
        }
    }

    public async Task<bool> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _recipeRepo.GetByIdAsync(id);
            if (entity is null) return false;

            _recipeRepo.SoftDelete(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao deletar receita com ID {id}", ex);
        }
    }

    // Métodos de busca com paginação
    public async Task<PaginationResult<RecipeListViewModel>> SearchRecipesAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedRecipes = await _recipeRepo.GetPagedWithIncludesAsync(request, 
                r => r.Title.ToLower().Contains(searchTerm.ToLower()) || 
                     r.Description != null && r.Description.ToLower().Contains(searchTerm.ToLower()),
                "User", "Category");
            
            var viewModels = pagedRecipes.Items.Select(ToListVm).ToList();
            
            return new PaginationResult<RecipeListViewModel>(
                viewModels, 
                pagedRecipes.TotalItems, 
                pagedRecipes.PageNumber, 
                pagedRecipes.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar receitas com termo '{searchTerm}'", ex);
        }
    }

    public async Task<PaginationResult<RecipeListViewModel>> GetRecipesByCategoryAsync(Guid categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedRecipes = await _recipeRepo.GetPagedWithIncludesAsync(request, 
                r => r.CategoryId == categoryId, "User", "Category");
            
            var viewModels = pagedRecipes.Items.Select(ToListVm).ToList();
            
            return new PaginationResult<RecipeListViewModel>(
                viewModels, 
                pagedRecipes.TotalItems, 
                pagedRecipes.PageNumber, 
                pagedRecipes.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar receitas da categoria {categoryId}", ex);
        }
    }

    public async Task<PaginationResult<RecipeListViewModel>> GetRecipesByUserAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedRecipes = await _recipeRepo.GetPagedWithIncludesAsync(request, 
                r => r.UserId == userId, "User", "Category");
            
            var viewModels = pagedRecipes.Items.Select(ToListVm).ToList();
            
            return new PaginationResult<RecipeListViewModel>(
                viewModels, 
                pagedRecipes.TotalItems, 
                pagedRecipes.PageNumber, 
                pagedRecipes.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar receitas do usuário {userId}", ex);
        }
    }

    public async Task<PaginationResult<RecipeListViewModel>> GetPublishedRecipesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedRecipes = await _recipeRepo.GetPagedWithIncludesAsync(request, 
                r => r.IsPublished, "User", "Category");
            
            var viewModels = pagedRecipes.Items.Select(ToListVm).ToList();
            
            return new PaginationResult<RecipeListViewModel>(
                viewModels, 
                pagedRecipes.TotalItems, 
                pagedRecipes.PageNumber, 
                pagedRecipes.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar receitas publicadas", ex);
        }
    }

    // Métodos de contagem
    public async Task<int> GetTotalRecipesCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _recipeRepo.CountAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao contar receitas", ex);
        }
    }

    public async Task<int> GetPublishedRecipesCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _recipeRepo.CountAsync(r => r.IsPublished);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao contar receitas publicadas", ex);
        }
    }

    public async Task<int> GetUserRecipesCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _recipeRepo.CountAsync(r => r.UserId == userId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao contar receitas do usuário {userId}", ex);
        }
    }

    // ----------------- Helpers -----------------

    private static RecipeListViewModel ToListVm(Recipe r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        Slug = r.Slug,
        ImageId = r.ImageId,
        ImageUrl = GetImageUrl(r.ImageId),
        IsPublished = r.IsPublished,
        PrepTime = r.PrepTime,
        Servings = r.Servings,
        Difficulty = r.Difficulty,
        UserName = r.User?.FullName ?? "Usuário",
        CategoryName = r.Category?.Name
    };

    private static RecipeDetailViewModel ToDetailVm(Recipe r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        Title = r.Title,
        Slug = r.Slug,
        Description = r.Description,
        PrepTime = r.PrepTime,
        Servings = r.Servings,
        Difficulty = r.Difficulty,
        ImageId = r.ImageId,
        ImageUrl = GetImageUrl(r.ImageId),
        IsPublished = r.IsPublished,
        UserName = r.User?.FullName ?? "Usuário",
        CategoryName = r.Category?.Name,
        Instructions = r.Instructions.Select(i => new InstructionViewModel
        {
            Id = i.Id,
            StepNumber = i.StepNumber,
            Content = i.Content
        }).ToList(),
        Ingredients = r.Ingredients.Select(i => new IngredientViewModel
        {
            Id = i.Id,
            Name = i.Name,
            Amount = i.Amount,
            Unit = i.Unit
        }).ToList(),
        TagNames = r.RecipeTags.Select(rt => rt.Tag.Name).ToList()
    };

    private static Instruction MapInstruction(InstructionViewModel vm) => new()
    {
        Id = vm.Id != Guid.Empty ? vm.Id : Guid.NewGuid(),
        StepNumber = vm.StepNumber,
        Content = vm.Content
        // RecipeId será definido pela entidade Recipe
    };

    private static Ingredient MapIngredient(IngredientViewModel vm) => new()
    {
        Id = vm.Id != Guid.Empty ? vm.Id : Guid.NewGuid(),
        Name = vm.Name,
        Amount = vm.Amount,
        Unit = vm.Unit
    };

    /// <summary>
    /// Processa lista de nomes de tags, criando novas se necessário
    /// </summary>
    private async Task<List<RecipeTag>> ProcessTagNamesAsync(List<string>? tagNames, CancellationToken cancellationToken = default)
    {
        var recipeTags = new List<RecipeTag>();
        
        if (tagNames == null || !tagNames.Any())
            return recipeTags;

        foreach (var tagName in tagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var normalizedName = tagName.Trim();
            
            // Buscar tag existente
            var existingTag = await _tagRepo.FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedName.ToLower());
            
            if (existingTag == null)
            {
                // Criar nova tag
                existingTag = new Tag { Name = normalizedName };
                await _tagRepo.AddAsync(existingTag);
                // Note: Não fazemos SaveChanges aqui, será feito no final junto com a receita
            }
            
            recipeTags.Add(new RecipeTag
            {
                TagId = existingTag.Id,
                RecipeId = Guid.Empty // Será definido pela entidade Recipe
            });
        }
        
        return recipeTags;
    }

    private static Instruction MapInstructionCreate(InstructionCreateViewModel vm) => new()
    {
        Id = Guid.NewGuid(),
        StepNumber = vm.StepNumber,
        Content = vm.Content
        // RecipeId será definido pela entidade Recipe
    };

    private static Ingredient MapIngredientCreate(IngredientCreateViewModel vm) => new()
    {
        Id = Guid.NewGuid(),
        Name = vm.Name,
        Amount = vm.Amount,
        Unit = vm.Unit
    };



    /// <summary>
    /// Gera URL da imagem baseada na configuração (método simples sem configuração por enquanto)
    /// </summary>
    private static string? GetImageUrl(Guid? imageId)
    {
        if (!imageId.HasValue) return null;
        
        return $"/api/Images/{imageId}";
    }

    /// <summary>
    /// Garante unicidade do slug com sufixos -2, -3, …
    /// </summary>
    private async Task<string> GenerateUniqueSlugAsync(string baseSlug, Guid? currentId = null)
    {
        var slug = Slugify(baseSlug);
        var i = 2;

        while (await _recipeRepo.AnyAsync(r =>
            r.Slug == slug &&
            (!currentId.HasValue || r.Id != currentId.Value)))
        {
            slug = $"{baseSlug}-{i}";
            i++;
        }

        return slug;
    }

    // Protege contra input estranho vindo de fora
    private static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var s = text.ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", " ").Trim();
        s = Regex.Replace(s, @"\s", "-");
        s = Regex.Replace(s, "-{2,}", "-").Trim('-');
        return s;
    }

    /// <summary>
    /// Combina expressões LINQ usando AND
    /// </summary>
    private static Expression<Func<Recipe, bool>> CombineExpressions(
        Expression<Func<Recipe, bool>> first, 
        Expression<Func<Recipe, bool>> second)
    {
        var parameter = first.Parameters[0];
        var body = Expression.AndAlso(first.Body, Expression.Invoke(second, parameter));
        return Expression.Lambda<Func<Recipe, bool>>(body, parameter);
    }
}