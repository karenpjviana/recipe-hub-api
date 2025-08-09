using Microsoft.AspNetCore.Mvc;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Auth;

namespace RecipeHub.API.Controllers;

/// <summary>
/// Controller responsável pela autenticação de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Realizar login no sistema
    /// </summary>
    /// <param name="loginViewModel">Dados de login (username e password)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Token JWT e informações do usuário</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseViewModel>> Login([FromBody] LoginViewModel loginViewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(loginViewModel, cancellationToken);
        
        if (result == null)
        {
            return Unauthorized(new { message = "Username ou senha inválidos" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Registrar novo usuário no sistema
    /// </summary>
    /// <param name="registerViewModel">Dados do novo usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Token JWT e informações do usuário criado</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseViewModel>> Register([FromBody] RegisterViewModel registerViewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await _authService.UsernameExistsAsync(registerViewModel.Username, cancellationToken))
        {
            return BadRequest(new { message = "Username já está em uso" });
        }

        var result = await _authService.RegisterAsync(registerViewModel, cancellationToken);
        
        if (result == null)
        {
            return BadRequest(new { message = "Erro ao criar usuário" });
        }

        return CreatedAtAction(nameof(Login), result);
    }

    /// <summary>
    /// Verificar disponibilidade de username
    /// </summary>
    /// <param name="username">Username a ser verificado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Indica se o username está disponível</returns>
    [HttpGet("check-username/{username}")]
    public async Task<ActionResult<bool>> CheckUsername(string username, CancellationToken cancellationToken)
    {
        var exists = await _authService.UsernameExistsAsync(username, cancellationToken);
        return Ok(new { exists });
    }
}
