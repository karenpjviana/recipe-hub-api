using RecipeHub.Domain.Common;
using RecipeHub.Application.ViewModels.User;

namespace RecipeHub.Application.Interfaces;

public interface IUserService
{
    // Métodos básicos
    Task<IEnumerable<UserListViewModel>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<PaginationResult<UserListViewModel>> GetUsersPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    

    Task<UserDetailViewModel?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDetailViewModel?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Guid> AddUserAsync(UserCreateViewModel request, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(Guid id, UserUpdateViewModel request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Métodos de contagem
    Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken = default);
}
