using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace ScanProWeb.Services.OrdenCompra
{
    public class OrdenCompraDto
    {
        public string Numero { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Proveedor { get; set; } = "";
        public string Estado { get; set; } = "";
        public int ProveedorId { get; set; }
        public int AlmacenId { get; set; }
        public List<OrdenCompraItemDto> Items { get; set; } = new();
    }

    public class OrdenCompraItemDto
    {
        public string ProductoId { get; set; } = "";
        public string Producto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }
    }

    public class OrdenCompraService(IHttpClientFactory factory, IJSRuntime js) : IOrdenCompraService
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

        public async Task<List<OrdenCompraDto>> GetOrdersAsync()
        {
            try
            {
                var client = factory.CreateClient("API");
                await ApplyAuthAsync(client);
                var res = await client.GetAsync("api/ordencompra/getorders");
                if (!res.IsSuccessStatusCode) return [];
                return await res.Content.ReadFromJsonAsync<List<OrdenCompraDto>>() ?? [];
            }
            catch
            {
                return [];
            }
        }

        public async Task<OrdenCompraDto?> GetByIdAsync(string numero)
        {
            try
            {
                var client = factory.CreateClient("API");
                await ApplyAuthAsync(client);
                var res = await client.GetAsync($"api/ordencompra/getorderbyid/{numero}");
                if (!res.IsSuccessStatusCode) return null;
                return await res.Content.ReadFromJsonAsync<OrdenCompraDto>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AddAsync(OrdenCompraDto dto)
        {
            try
            {
                var client = factory.CreateClient("API");
                await ApplyAuthAsync(client);
                var res = await client.PostAsJsonAsync("api/ordencompra/addorder", dto);
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string numero)
        {
            try
            {
                var client = factory.CreateClient("API");
                await ApplyAuthAsync(client);
                var res = await client.DeleteAsync($"api/ordencompra/deleteorder/{numero}");
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetNextNumAsync()
        {
            try
            {
                var client = factory.CreateClient("API");
                await ApplyAuthAsync(client);
                var res = await client.GetAsync("api/ordencompra/getnextnum");
                res.EnsureSuccessStatusCode();
                var obj = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (obj != null && obj.TryGetValue("numero", out var numero) && !string.IsNullOrWhiteSpace(numero))
                    return numero;
                return await CalcularSiguienteNumeroLocalAsync();
            }
            catch
            {
                return await CalcularSiguienteNumeroLocalAsync();
            }
        }

        private async Task<string> CalcularSiguienteNumeroLocalAsync()
        {
            try
            {
                var ordenes = await GetOrdersAsync();
                if (ordenes.Count == 0) return "PO-0001";
                var max = 0;
                foreach (var o in ordenes)
                {
                    var match = Regex.Match(o.Numero ?? "", @"^PO-(\d+)$");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var n) && n > max)
                        max = n;
                }
                return $"PO-{max + 1:0000}";
            }
            catch
            {
                return "PO-0001";
            }
        }
    }
}
