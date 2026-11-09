using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services.Config;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigController(IConfigService service) : ControllerBase
{
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories() => Ok(await service.GetCategoriesAsync());

    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateCategory([FromBody] Category category)
    {
        var created = await service.CreateCategoryAsync(category);
        return created is null ? Conflict("Ya existe una categoría con ese nombre o el nombre está vacío.") : Ok(created);
    }

    [HttpPut("categories/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateCategory(int id, [FromBody] Category category)
    {
        var updated = await service.UpdateCategoryAsync(id, category);
        return updated is null ? Conflict("La categoría no existe, el nombre está vacío o ya está siendo utilizado.") : Ok(updated);
    }

    [HttpDelete("categories/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id) =>
        await service.DeleteCategoryAsync(id) ? NoContent() : NotFound();

    [HttpGet("units")]
    public async Task<ActionResult> GetUnits() => Ok(await service.GetProductUnitsAsync());

    [HttpPost("units")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateUnit([FromBody] ProductUnit unit)
    {
        var created = await service.CreateProductUnitAsync(unit);
        return created is null ? Conflict("Ya existe una unidad con ese nombre o el nombre está vacío.") : Ok(created);
    }

    [HttpPut("units/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateUnit(int id, [FromBody] ProductUnit unit)
    {
        var updated = await service.UpdateProductUnitAsync(id, unit);
        return updated is null ? Conflict("La unidad no existe, el nombre está vacío o ya está siendo utilizado.") : Ok(updated);
    }

    [HttpDelete("units/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUnit(int id) =>
        await service.DeleteProductUnitAsync(id) ? NoContent() : NotFound();
}