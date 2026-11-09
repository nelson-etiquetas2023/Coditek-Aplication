using System.Collections.ObjectModel;

namespace ScanProMaui.Services;

public interface IApiConfigService
{
    Task<ObservableCollection<ConfigDto>> GetConfigsAsync();
    Task<bool> CreateConfigAsync(ConfigDto config);
    Task<bool> UpdateConfigAsync(int id, ConfigDto config);
    Task<bool> DeleteConfigAsync(int id);
}

public class ApiConfigService : IApiConfigService
{
    private readonly HttpClient _httpClient;

    public ApiConfigService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ObservableCollection<ConfigDto>> GetConfigsAsync()
    {
        var response = await _httpClient.GetAsync("https://localhost:7220/api/configs");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var configs = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<ConfigDto>>(content);
            return configs ?? new ObservableCollection<ConfigDto>();
        }
        return new ObservableCollection<ConfigDto>();
    }

    public async Task<bool> CreateConfigAsync(ConfigDto config)
    {
        var content = System.Text.Json.JsonSerializer.Serialize(config);
        var body = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://localhost:7220/api/configs", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateConfigAsync(int id, ConfigDto config)
    {
        var content = System.Text.Json.JsonSerializer.Serialize(config);
        var body = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"https://localhost:7220/api/configs/{id}", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteConfigAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"https://localhost:7220/api/configs/{id}");
        return response.IsSuccessStatusCode;
    }
}