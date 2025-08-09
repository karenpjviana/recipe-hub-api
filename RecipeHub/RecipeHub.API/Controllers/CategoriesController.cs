using Microsoft.AspNetCore.Mvc;
using RecipeHub.Domain.Common;
using RecipeHub.Application.Interfaces;
using RecipeHub.Application.ViewModels.Category;
using RecipeHub.API.Attributes;

namespace RecipeHub.API.Controllers;

/// <summary>
/// Controller para gerenciamento de categorias de receitas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    public CategoriesController(ICategoryService service) => _service = service;

    /// <summary>
    /// Listar todas as categorias (para dropdowns)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryListViewModel>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _service.GetAllCategoriesAsync(cancellationToken));

    /// <summary>
    /// Buscar categoria por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDetailViewModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vm = await _service.GetCategoryByIdAsync(id, cancellationToken);
        return vm is null ? NotFound() : Ok(vm);
    }

    /// <summary>
    /// Criar nova categoria (requer permissão de administrador)
    /// </summary>
    [HttpPost]
    [AdminAuthorize]
    public async Task<ActionResult<Guid>> Create([FromBody] CategoryCreateViewModel body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var id = await _service.AddCategoryAsync(body, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Atualizar categoria (requer permissão de administrador)
    /// </summary>
    [HttpPut("{id:guid}")]
    [AdminAuthorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryUpdateViewModel body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        return await _service.UpdateCategoryAsync(id, body, cancellationToken) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Deletar categoria (requer permissão de administrador)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [AdminAuthorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await _service.DeleteCategoryAsync(id, cancellationToken) ? NoContent() : NotFound();
}
