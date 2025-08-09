using RecipeHub.Application.ViewModels.Auth;

namespace RecipeHub.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseViewModel?> LoginAsync(LoginViewModel loginViewModel, CancellationToken cancellationToken = default);
        Task<AuthResponseViewModel?> RegisterAsync(RegisterViewModel registerViewModel, CancellationToken cancellationToken = default);
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    }
}
