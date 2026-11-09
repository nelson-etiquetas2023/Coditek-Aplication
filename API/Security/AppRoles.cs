namespace API.Security;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Invitado = "Invitado";

    public static readonly string[] All = [Admin, User, Invitado];

    public static bool IsValid(string? role) =>
        !string.IsNullOrWhiteSpace(role) && All.Contains(role.Trim());
}