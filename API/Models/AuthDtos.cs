using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class UserLogin
{
    [StringLength(256)]
    public string Identifier { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; set; }

    [Required]
    [StringLength(256)]
    public string Password { get; set; } = string.Empty;

    public string ResolveIdentifier() =>
        string.IsNullOrWhiteSpace(Identifier) ? Email?.Trim() ?? string.Empty : Identifier.Trim();
}

public class UserRegister
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}