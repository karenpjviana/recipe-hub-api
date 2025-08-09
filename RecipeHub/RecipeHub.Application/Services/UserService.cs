using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.User;
using RecipeHub.Application.ViewModels.Recipe;
using RecipeHub.Domain.Entities;
using RecipeHub.Domain.Interfaces;

namespace RecipeHub.Application.Services;

public class UserService : IUserService
{
    private readonly IRepositoryBase<User> _userRepo;
    private readonly IUnitOfWork _uow;

    public UserService(IRepositoryBase<User> userRepo, IUnitOfWork uow)
    {
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<IEnumerable<UserListViewModel>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepo.GetAllWithIncludesAsync("Recipes");
            return users.Select(ToListVm).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar usuários", ex);
        }
    }

    public async Task<PaginationResult<UserListViewModel>> GetUsersPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedUsers = await _userRepo.GetPagedWithIncludesAsync(request, "Recipes");
            var viewModels = pagedUsers.Items.Select(ToListVm).ToList();
            
            return new PaginationResult<UserListViewModel>(
                viewModels, 
                pagedUsers.TotalItems, 
                pagedUsers.PageNumber, 
                pagedUsers.PageSize
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar usuários paginados", ex);
        }
    }

    public async Task<UserDetailViewModel?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _userRepo.GetByIdWithIncludesAsync(id, "Recipes", "Favorites", "Reviews");
            return entity is null ? null : ToDetailVm(entity);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar usuário com ID {id}", ex);
        }
    }

    public async Task<UserDetailViewModel?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _userRepo.FirstOrDefaultAsync(u => u.Username.ToLower().Contains(username.ToLower()));
            if (entity == null) return null;
            
            var fullEntity = await _userRepo.GetByIdWithIncludesAsync(entity.Id, "Recipes", "Favorites", "Reviews");
            return fullEntity is null ? null : ToDetailVm(fullEntity);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar usuário com username {username}", ex);
        }
    }

    public async Task<Guid> AddUserAsync(UserCreateViewModel vm, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new User
            {
                Username = vm.Username,
                FullName = vm.FullName,
                AvatarImageId = vm.AvatarImageId
            };

            await _userRepo.AddAsync(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao criar usuário", ex);
        }
    }

    public async Task<bool> UpdateUserAsync(Guid id, UserUpdateViewModel vm, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _userRepo.GetByIdAsync(id);
            if (entity is null) return false;

            entity.Username = vm.Username;
            entity.FullName = vm.FullName;
            entity.AvatarImageId = vm.AvatarImageId;

            _userRepo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar usuário com ID {id}", ex);
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _userRepo.GetByIdAsync(id);
            if (entity is null) return false;

            _userRepo.SoftDelete(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao deletar usuário com ID {id}", ex);
        }
    }

    public async Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _userRepo.CountAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao contar usuários", ex);
        }
    }

    // ----------------- Helpers -----------------

    private static UserListViewModel ToListVm(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        AvatarImageId = u.AvatarImageId,
        AvatarUrl = u.AvatarImageId.HasValue ? $"/api/Images/{u.AvatarImageId}" : null,
        RecipesCount = u.Recipes?.Count ?? 0
    };

    private static UserDetailViewModel ToDetailVm(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        AvatarImageId = u.AvatarImageId,
        AvatarUrl = u.AvatarImageId.HasValue ? $"/api/Images/{u.AvatarImageId}" : null,
        RecipesCount = u.Recipes?.Count ?? 0,
        FavoritesCount = u.Favorites?.Count ?? 0,
        ReviewsCount = u.Reviews?.Count ?? 0,
        RecentRecipes = u.Recipes?.Take(5).Select(r => new RecipeListViewModel
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
