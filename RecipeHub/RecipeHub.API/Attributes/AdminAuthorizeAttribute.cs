using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace RecipeHub.API.Attributes;

/// <summary>
/// Autorização específica para administradores
/// </summary>
public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Verificar se o usuário está autenticado
        if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Verificar se tem a role Admin
        var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin")
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
