using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace ScanProWeb;

public class ApiLabelService(IHttpClientFactory factory, IJSRuntime js)
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<string?> GetTokenAsync()
    {
        try { return await js.InvokeAsync<string?>("localStorage.getItem", "authToken"); }
        catch { return null; }
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        var client = factory.CreateClient("API");
        var token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<List<LabelTemplateDto>> GetAllAsync()
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            return await client.GetFromJsonAsync<List<LabelTemplateDto>>("api/labels", jsonOptions) ?? [];
        }
        catch { return []; }
    }

    public async Task<LabelTemplateDto?> GetByIdAsync(int id)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            return await client.GetFromJsonAsync<LabelTemplateDto>($"api/labels/{id}", jsonOptions);
        }
        catch { return null; }
    }

    public async Task<string?> GetNextCodigoAsync()
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var dto = await client.GetFromJsonAsync<NextCodigoDto>("api/labels/next-code", jsonOptions);
            return dto?.Codigo;
        }
        catch { return null; }
    }

    public async Task<(bool Success, string? Error, LabelTemplateDto? Data)> CreateAsync(LabelTemplateDto label)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.PostAsJsonAsync("api/labels", label, jsonOptions);
            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) return (false, string.IsNullOrWhiteSpace(body) ? res.ReasonPhrase : body, null);
            return (true, null, JsonSerializer.Deserialize<LabelTemplateDto>(body, jsonOptions));
        }
        catch (Exception ex) { return (false, ex.Message, null); }
    }

    public async Task<(bool Success, string? Error, LabelTemplateDto? Data)> UpdateAsync(int id, LabelTemplateDto label)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.PutAsJsonAsync($"api/labels/{id}", label, jsonOptions);
            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) return (false, string.IsNullOrWhiteSpace(body) ? res.ReasonPhrase : body, null);
            return (true, null, JsonSerializer.Deserialize<LabelTemplateDto>(body, jsonOptions));
        }
        catch (Exception ex) { return (false, ex.Message, null); }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.DeleteAsync($"api/labels/{id}");
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(body) ? res.ReasonPhrase : body);
            }
            return (true, null);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Success, string? DataUrl, string? Error)> RenderPreviewAsync(RenderRequest req)
    {
        try
        {
            var client = factory.CreateClient("API");
            var res = await client.PostAsJsonAsync("api/labels/render", req, jsonOptions);
            var bytes = await res.Content.ReadAsByteArrayAsync();
            if (!res.IsSuccessStatusCode)
            {
                var msg = Encoding.UTF8.GetString(bytes);
                return (false, null, string.IsNullOrWhiteSpace(msg) ? res.ReasonPhrase : msg);
            }
            return (true, $"data:image/png;base64,{Convert.ToBase64String(bytes)}", null);
        }
        catch (Exception ex) { return (false, null, ex.Message); }
    }

    public class LabelTemplateDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La descripción de la etiqueta es obligatoria.")]
        [System.ComponentModel.DataAnnotations.StringLength(1000)]
        public string Descripcion { get; set; } = "";
        public decimal WidthMm { get; set; }
        public decimal LengthMm { get; set; }
        public int CantidadPorRollo { get; set; }
        public string CodigoZpl { get; set; } = "";
        public int Dpmm { get; set; } = 8;
        /// <summary>Tipo de material: Transferencia o Directo.</summary>
        public string TipoMaterial { get; set; } = "Transferencia";
    }

    public record RenderRequest(string Zpl, decimal WidthMm, decimal LengthMm, int Dpmm = 8, int Index = 0);

    public record NextCodigoDto(string Codigo);
}
