using Microsoft.Extensions.Http;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace ScanProWeb;

public class ApiConfigService(IHttpClientFactory factory, IJSRuntime js)
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

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.GetAsync("api/config/categories");
            if (!res.IsSuccessStatusCode) return [];
            return await res.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? [];
        }
        catch { return []; }
    }

    public async Task<CategoryDto?> CreateCategoryAsync(CategoryDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PostAsJsonAsync("api/config/categories", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<CategoryDto>();
        }
        catch { return null; }
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, CategoryDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PutAsJsonAsync($"api/config/categories/{id}", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<CategoryDto>();
        }
        catch { return null; }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.DeleteAsync($"api/config/categories/{id}");
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<List<ProductUnitDto>> GetProductUnitsAsync()
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.GetAsync("api/config/units");
            if (!res.IsSuccessStatusCode) return [];
            return await res.Content.ReadFromJsonAsync<List<ProductUnitDto>>() ?? [];
        }
        catch { return []; }
    }

    public async Task<ProductUnitDto?> CreateProductUnitAsync(ProductUnitDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PostAsJsonAsync("api/config/units", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<ProductUnitDto>();
        }
        catch { return null; }
    }

    public async Task<ProductUnitDto?> UpdateProductUnitAsync(int id, ProductUnitDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PutAsJsonAsync($"api/config/units/{id}", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<ProductUnitDto>();
        }
        catch { return null; }
    }

    public async Task<bool> DeleteProductUnitAsync(int id)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.DeleteAsync($"api/config/units/{id}");
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task EnsureLoginAsync()
    {
        _token = await GetTokenAsync();
    }

    public class CategoryDto { public int Id { get; set; } public string Name { get; set; } = ""; public string Description { get; set; } = ""; public bool IsActive { get; set; } = true; }
    public class ProductUnitDto { public int Id { get; set; } public string Name { get; set; } = ""; public string Description { get; set; } = ""; public bool IsActive { get; set; } = true; }
    public class ConfigDto { public int Id { get; set; } public string Key { get; set; } = ""; public string Value { get; set; } = ""; public string Description { get; set; } = ""; }

    // DATOS EMPRESA - NUEVO
    public async Task<List<CompanyDto>> GetCompanyDataAsync()
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.GetAsync("api/config/company");
            if (!res.IsSuccessStatusCode) return new();
            return await res.Content.ReadFromJsonAsync<List<CompanyDto>>() ?? new();
        }
        catch { return new(); }
    }

    public async Task<CompanyDto?> CreateCompanyDataAsync(CompanyDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PostAsJsonAsync("api/config/company", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<CompanyDto>();
        }
        catch { return null; }
    }

    public async Task<CompanyDto?> UpdateCompanyDataAsync(int id, CompanyDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PutAsJsonAsync($"api/config/company/{id}", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<CompanyDto>();
        }
        catch { return null; }
    }

    public async Task<bool> DeleteCompanyDataAsync(int id)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.DeleteAsync($"api/config/company/{id}");
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public class CompanyDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = "";
        public string NombreComercial { get; set; } = "";
        public string NIT { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Email { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string Pais { get; set; } = "";
        public string? LogoUrl { get; set; }
        public DateTime FechaFundacion { get; set; } = DateTime.Now;
    }

    public async Task<List<ConfigDto>> GetConfigsAsync()
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.GetAsync("api/config");
            if (!res.IsSuccessStatusCode) return new();
            return await res.Content.ReadFromJsonAsync<List<ConfigDto>>() ?? new();
        }
        catch { return new(); }
    }

    public async Task<ConfigDto?> CreateConfigAsync(ConfigDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PostAsJsonAsync("api/config", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<ConfigDto>();
        }
        catch { return null; }
    }

    public async Task<ConfigDto?> UpdateConfigAsync(int id, ConfigDto dto)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.PutAsJsonAsync($"api/config/{id}", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<ConfigDto>();
        }
        catch { return null; }
    }

    public async Task<bool> DeleteConfigAsync(int id)
    {
        await EnsureLoginAsync();
        try
        {
            var client = factory.CreateClient("API");
            await ApplyAuthAsync(client);
            var res = await client.DeleteAsync($"api/config/{id}");
            return res.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}