namespace ScanProMaui.Models;

public class UserDto
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Nickname { get; set; }
    public string[]? Roles { get; set; }
}