using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace ScanProWeb;

public class ApiEnterpriseService(IHttpClientFactory factory, IJSRuntime js)
{
    private string? _token;

    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private async Task<string?> GetTokenAsync()
    {
        try
        {
            var token = await js.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token)) _token = token;
        }
        catch
        {
            // Prerendering u otros errores
        }
        return _token;
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        var client = factory.CreateClient("API");
        await ApplyAuthAsync(client);
        return client;
    }

    private async Task ApplyAuthAsync(HttpClient client)
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<Enterprise?> GetEnterpriseAsync()
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.GetAsync("api/enterprises/getenterprise");
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
            return JsonSerializer.Deserialize<Enterprise>(json, jsonOptions);
        }
        catch { return null; }
    }

    public async Task<(Enterprise? Data, string? Error)> GetEnterpriseWithDetailsAsync()
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.GetAsync("api/enterprises/getenterprise");
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                return (null, string.IsNullOrWhiteSpace(body) ? res.ReasonPhrase : body);
            }
            var json = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null") return (null, null);
            return (JsonSerializer.Deserialize<Enterprise>(json, jsonOptions), null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<List<Enterprise>> GetAllAsync()
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.GetAsync("api/enterprises/getall");
            if (!res.IsSuccessStatusCode) return [];
            var json = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return [];
            return JsonSerializer.Deserialize<List<Enterprise>>(json, jsonOptions) ?? [];
        }
        catch { return []; }
    }

    public async Task<bool> CreateEnterpriseAsync(Enterprise enterprise)
    {
        var result = await CreateEnterpriseWithDetailsAsync(enterprise);
        return result.Success;
    }

    public async Task<(bool Success, string? Error)> CreateEnterpriseWithDetailsAsync(Enterprise enterprise)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var json = JsonSerializer.Serialize(enterprise, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await client.PostAsync("api/enterprises/createenterprise", content);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"CreateEnterprise failed: {res.StatusCode} - {body}");
                return (false, string.IsNullOrWhiteSpace(body) ? res.ReasonPhrase : body);
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateEnterprise exception: {ex}");
            return (false, ex.Message);
        }
    }

    public async Task<bool> UpdateEnterpriseAsync(int id, Enterprise enterprise)
    {
        var result = await UpdateEnterpriseWithDetailsAsync(id, enterprise);
        return result.Success;
    }

    public async Task<(bool Success, string? Error)> UpdateEnterpriseWithDetailsAsync(int id, Enterprise enterprise)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            enterprise.enterprise_id = id;
            var json = JsonSerializer.Serialize(enterprise, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await client.PutAsync("api/enterprises/updateenterprise", content);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"UpdateEnterprise failed: {res.StatusCode} - {body}");
                return (false, string.IsNullOrWhiteSpace(body) ? res.ReasonPhrase : body);
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateEnterprise exception: {ex}");
            return (false, ex.Message);
        }
    }

    public class Enterprise
    {
        public int enterprise_id { get; set; }
        public byte[]? Logo { get; set; }
        public string LogoContentType { get; set; } = "image/png";
        public string Codigo_Empresa { get; set; } = "";
        public string Nombre_Empresa { get; set; } = "";
        public string Tipo_Empresa { get; set; } = "";
        public string Registro_Fiscal { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Correo { get; set; } = "";
        public double Latitud { get; set; } = 0;
        public double Longitud { get; set; } = 0;
        public string Persona_Contacto { get; set; } = "";
    }
}
