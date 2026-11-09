using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services.Users;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController(IUsersService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var users = await service.GetAllAsync();
        var result = new List<object>();
        foreach (var u in users)
        {
            result.Add(new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.Nickname,
                u.PhotoData,
                Roles = await service.GetRolesAsync(u)
            });
        }
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Get(int id)
    {
        var u = await service.GetByIdAsync(id);
        if (u is null) return NotFound();
        return Ok(new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.Nickname,
            u.PhotoData,
            Roles = await service.GetRolesAsync(u)
        });
    }

    [HttpPost]
    public async Task<ActionResult> Create(UserCreateRequest req)
    {
        var user = new User
        {
            UserName = req.UserName,
            Email = req.Email,
            Nickname = req.Nickname,
            PhotoData = req.PhotoData
        };
        var (ok, error) = await service.CreateAsync(user, req.Password, req.Role);
        if (!ok) return BadRequest(new { error });
        return CreatedAtAction(nameof(Get), new { id = user.Id }, new { user.Id, user.UserName, user.Email, req.Role });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UserUpdateRequest req)
    {
        var user = new User
        {
            Id = id,
            UserName = req.UserName,
            Email = req.Email,
            Nickname = req.Nickname,
            PhotoData = req.PhotoData
        };
        if (!await service.UpdateAsync(id, user, req.Role, req.NewPassword)) return BadRequest();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await service.DeleteAsync(id)) return NotFound();
        return NoContent();
    }
}

