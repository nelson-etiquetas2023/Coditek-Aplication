using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using API.Models;
using API.Security;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<ServiceResponse<string>>> Login([FromBody] UserLogin request)
    {
        var response = new ServiceResponse<string>();
        var identifier = request.ResolveIdentifier();
        if (string.IsNullOrWhiteSpace(identifier))
        {
            response.Success = false;
            response.Message = "El identificador (correo o nickname) es un campo requerido.";
            return BadRequest(response);
        }

        var normalized = identifier.Trim().ToLowerInvariant();
        var matches = userManager.Users
            .Where(u => u.Email!.ToLower() == normalized || u.Nickname.ToLower() == normalized || u.UserName!.ToLower() == normalized)
            .Take(2)
            .ToList();

        if (matches.Count != 1)
        {
            response.Success = false;
            response.Message = "Credenciales incorrectas.";
            return BadRequest(response);
        }

        var user = matches[0];
        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            response.Success = false;
            response.Message = "Credenciales incorrectas.";
            return BadRequest(response);
        }

        var roles = await userManager.GetRolesAsync(user);
        response.Data = CreateToken(user, roles);
        response.Message = "login ok.";
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<ServiceResponse<int>>> Register([FromBody] UserRegister request)
    {
        var response = new ServiceResponse<int>();
        var role = request.Role.Trim();
        if (!AppRoles.IsValid(role))
        {
            response.Success = false;
            response.Message = "El rol indicado no es válido.";
            return BadRequest(response);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedNickname = request.Nickname.Trim().ToLowerInvariant();
        var exists = await userManager.Users.AnyAsync(u =>
            u.Email!.ToLower() == normalizedEmail ||
            u.Nickname.ToLower() == normalizedNickname ||
            u.UserName!.ToLower() == request.UserName.Trim().ToLowerInvariant());
        if (exists)
        {
            response.Success = false;
            response.Message = "El correo o nickname ya existe.";
            return BadRequest(response);
        }

        if (request.Password != request.ConfirmPassword)
        {
            response.Success = false;
            response.Message = "no coinciden el password...";
            return BadRequest(response);
        }

        var user = new User
        {
            Email = request.Email.Trim(),
            Nickname = normalizedNickname,
            UserName = request.UserName.Trim()
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            response.Success = false;
            response.Message = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return BadRequest(response);
        }

        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        await userManager.AddToRoleAsync(user, role);

        response.Data = user.Id;
        response.Success = true;
        response.Message = "Usuario creado exitosamente.";
        return Ok(response);
    }

    private string CreateToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("nickname", user.Nickname)
        };
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

        var mysecret = config["AppSettings:Token"] ?? "mytopsecretkey*etiquetas.com*appswebritramacloud1632026*###keyapp###";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mysecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}