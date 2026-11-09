using System.Collections.ObjectModel;

namespace ScanProMaui.Services;

public interface IApiProductService
{
    Task<ObservableCollection<ProductDto>> GetProductsAsync();
    Task<bool> CreateProductAsync(ProductDto product);
    Task<bool> UpdateProductAsync(int id, ProductDto product);
    Task<bool> DeleteProductAsync(int id);
}

public class ApiProductService : IApiProductService
{
    private readonly HttpClient _httpClient;

    public ApiProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ObservableCollection<ProductDto>> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync("https://localhost:7220/api/products");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            // Deserializar JSON a colección
            var products = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<ProductDto>>(content);
            return products ?? new ObservableCollection<ProductDto>();
        }
        return new ObservableCollection<ProductDto>();
    }

    public async Task<bool> CreateProductAsync(ProductDto product)
    {
        var content = System.Text.Json.JsonSerializer.Serialize(product);
        var body = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://localhost:7220/api/products", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProductAsync(int id, ProductDto product)
    {
        var content = System.Text.Json.JsonSerializer.Serialize(product);
        var body = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"https://localhost:7220/api/products/{id}", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"https://localhost:7220/api/products/{id}");
        return response.IsSuccessStatusCode;
    }
}