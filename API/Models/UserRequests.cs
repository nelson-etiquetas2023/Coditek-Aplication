using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class UserCreateRequest
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string? PhotoData { get; set; }
}

public class UserUpdateRequest
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? NewPassword { get; set; }
    public string? PhotoData { get; set; }
}