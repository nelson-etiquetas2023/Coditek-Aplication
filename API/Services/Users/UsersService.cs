using API.Models;
using Microsoft.AspNetCore.Identity;

namespace API.Services.Users;

public interface IUsersService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<(bool ok, string? error)> CreateAsync(User user, string password, string role);
    Task<bool> UpdateAsync(int id, User user, string? role, string? newPassword = null);
    Task<bool> DeleteAsync(int id);
    Task<IList<string>> GetRolesAsync(User user);
}

public class UsersService(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager) : IUsersService
{
    public async Task<List<User>> GetAllAsync() =>
        userManager.Users.ToList();

    public async Task<User?> GetByIdAsync(int id) =>
        await userManager.FindByIdAsync(id.ToString());

    public async Task<(bool ok, string? error)> CreateAsync(User user, string password, string role)
    {
        user.DateCreated = DateTime.Now;
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        if (!string.IsNullOrWhiteSpace(role))
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            await userManager.AddToRoleAsync(user, role);
        }
        return (true, null);
    }

    public async Task<bool> UpdateAsync(int id, User user, string? role, string? newPassword = null)
    {
        var existing = await userManager.FindByIdAsync(id.ToString());
        if (existing is null) return false;

        existing.Email = user.Email;
        existing.UserName = user.UserName;
        existing.Nickname = user.Nickname;
        existing.PhotoData = user.PhotoData;

        var result = await userManager.UpdateAsync(existing);
        if (!result.Succeeded) return false;

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            var remove = await userManager.RemovePasswordAsync(existing);
            if (remove.Succeeded)
                await userManager.AddPasswordAsync(existing, newPassword);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var currentRoles = await userManager.GetRolesAsync(existing);
            await userManager.RemoveFromRolesAsync(existing, currentRoles);
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            await userManager.AddToRoleAsync(existing, role);
        }
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await userManager.FindByIdAsync(id.ToString());
        if (existing is null) return false;
        var result = await userManager.DeleteAsync(existing);
        return result.Succeeded;
    }

    public async Task<IList<string>> GetRolesAsync(User user) =>
        await userManager.GetRolesAsync(user);
}