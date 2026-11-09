using Microsoft.AspNetCore.Identity;

namespace API.Models;

public class User : IdentityUser<int>
{
    public string Nickname { get; set; } = string.Empty;
    public string? PhotoData { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.Now;
}