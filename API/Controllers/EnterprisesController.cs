using API.Models;
using API.Services.Enterprises;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnterprisesController(IEnterprisesService service) : ControllerBase
{
    [HttpGet]
    [Route("getenterprise")]
    public async Task<IActionResult> GetEnterpriseAsync()
    {
        var enterprise = await service.GetEnterpriseAsync();
        return Ok(enterprise);
    }

    [HttpGet]
    [Route("getall")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await service.GetAllAsync());
    }

    [HttpPost]
    [Route("createenterprise")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEnterpriseAsync(Enterprise enterprise)
    {
        var created = await service.CreateEnterpriseAsync(enterprise);
        return CreatedAtAction(nameof(GetEnterpriseAsync), new { }, created);
    }

    [HttpPut]
    [Route("updateenterprise")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEnterpriseAsync([FromBody] Enterprise enterprise)
    {
        if (enterprise is null || enterprise.enterprise_id <= 0)
            return BadRequest("Invalid enterprise data");

        var updated = await service.UpdateEnterpriseAsync(enterprise.enterprise_id, enterprise);
        if (updated == null)
            return NotFound("Empresa no encontrada.");
        return Ok(updated);
    }
}