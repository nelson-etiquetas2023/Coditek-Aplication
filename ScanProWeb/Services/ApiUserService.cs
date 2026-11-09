using Microsoft.Extensions.Http;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace ScanProWeb;

public class ApiUserService(IHttpClientFactory factory, IJSRuntime js)
{
    private string? _token;

    private async Task<string?> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_token)) return _token;
        try { _token = await js.InvokeAsync<string?>("localStorage.getItem", "authToken"); }
        catch { _token = null; }
        return _token;
    }

    private async Task ApplyAuthAsync(HttpClient client)
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task EnsureLoginAsync()
    {
        _token = await GetTokenAsync();
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.GetAsync("api/users");
            if (!res.IsSuccessStatusCode) return [];
            var data = await res.Content.ReadFromJsonAsync<List<UserDto>>();
            return data ?? [];
        }
        catch { return []; }
    }

    public async Task<bool> CreateUserAsync(UserCreateDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PostAsJsonAsync("api/users", dto);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"CreateUser failed: {res.StatusCode} {body}");
            }
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateUser exception: {ex}");
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(int id, UserUpdateDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PutAsJsonAsync($"api/users/{id}", dto);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"UpdateUser failed: {res.StatusCode} {body}");
            }
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateUser exception: {ex}");
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.DeleteAsync($"api/users/{id}");
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"DeleteUser failed: {res.StatusCode} {body}");
            }
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteUser exception: {ex}");
            return false;
        }
    }

    public class LoginResponse { public bool Success { get; set; } public string? Message { get; set; } public string? Data { get; set; } }
    public class UserDto { public int Id { get; set; } public string? UserName { get; set; } public string? Email { get; set; } public string? Nickname { get; set; } public string? PhotoData { get; set; } public List<string>? Roles { get; set; } }
    public class UserCreateDto { public string UserName { get; set; } = ""; public string Email { get; set; } = ""; public string Password { get; set; } = ""; public string Nickname { get; set; } = ""; public string Role { get; set; } = "User"; public string? PhotoData { get; set; } }
    public class UserUpdateDto { public string UserName { get; set; } = ""; public string Email { get; set; } = ""; public string Nickname { get; set; } = ""; public string? Role { get; set; } public string? NewPassword { get; set; } public string? PhotoData { get; set; } }
}

