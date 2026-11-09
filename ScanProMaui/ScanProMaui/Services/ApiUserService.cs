using System.Collections.ObjectModel;

namespace ScanProMaui.Services;

public interface IApiUserService
{
    Task<ObservableCollection<UserDto>> GetUsersAsync();
    Task<bool> CreateUserAsync(UserDto user);
    Task<bool> UpdateUserAsync(int id, UserDto user);
    Task<bool> DeleteUserAsync(int id);
}

public class ApiUserService : IApiUserService
{
    private readonly HttpClient _httpClient;

    public ApiUserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ObservableCollection<UserDto>> GetUsersAsync()
    {
        var response = await _httpClient.GetAsync("https://localhost:7220/api/users");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var users = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<UserDto>>(content);
            return users ?? new ObservableCollection<UserDto>();
        }
        return new ObservableCollection<UserDto>();
    }

    public async Task<bool> CreateUserAsync(UserDto user)
    {
        var content = System.Text.Json.JsonSerializer.Serialize(user);
        var body = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://localhost:7220/api/users", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateUserAsync(int id, UserDto user)
    {
        var content = System.Text.Json.JsonSerializer.Serialize(user);
        var body = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"https://localhost:7220/api/users/{id}", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"https://localhost:7220/api/users/{id}");
        return response.IsSuccessStatusCode;
    }
}