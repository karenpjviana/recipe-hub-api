using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Auth;
using RecipeHub.Domain.Entities;
using RecipeHub.Domain.Interfaces;

namespace RecipeHub.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepositoryBase<User> _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _configuration;

        public AuthService(IRepositoryBase<User> userRepo, IUnitOfWork uow, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _uow = uow;
            _configuration = configuration;
        }

        public async Task<AuthResponseViewModel?> LoginAsync(LoginViewModel loginViewModel, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepo.FirstOrDefaultAsync(u => u.Username == loginViewModel.Username);
                
                if (user == null || !VerifyPassword(loginViewModel.Password, user.PasswordHash))
                {
                    return null;
                }

                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                return new AuthResponseViewModel
                {
                    Token = token,
                    Username = user.Username,
                    FullName = user.FullName,
                    UserId = user.Id,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<AuthResponseViewModel?> RegisterAsync(RegisterViewModel registerViewModel, CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar se username já existe
                if (await UsernameExistsAsync(registerViewModel.Username, cancellationToken))
                {
                    return null;
                }

                var user = new User
                {
                    Username = registerViewModel.Username,
                    FullName = registerViewModel.FullName,
                    PasswordHash = HashPassword(registerViewModel.Password),
                    AvatarImageId = registerViewModel.AvatarImageId
                };

                await _userRepo.AddAsync(user);
                await _uow.SaveChangesAsync(cancellationToken);

                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                return new AuthResponseViewModel
                {
                    Token = token,
                    Username = user.Username,
                    FullName = user.FullName,
                    UserId = user.Id,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _userRepo.AnyAsync(u => u.Username == username);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-for-jwt-token-generation-recipe-hub-api";
            var issuer = jwtSettings["Issuer"] ?? "RecipeHubAPI";
            var audience = jwtSettings["Audience"] ?? "RecipeHubUsers";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            // Em produção, use BCrypt ou similar
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            // Em produção, use BCrypt ou similar
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
