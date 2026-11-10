using Microsoft.Extensions.Http;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace ScanProWeb;

public class ApiProductService(IHttpClientFactory factory, IJSRuntime js)
{
    private string? _token;

    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
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

    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.GetAsync("api/products");
            if (!res.IsSuccessStatusCode) return [];
            return await res.Content.ReadFromJsonAsync<List<Product>>() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<Product?> GetProductAsync(int id)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.GetAsync($"api/products/{id}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<Product>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateProductAsync(Product product)
    {
        var result = await CreateProductWithDetailsAsync(product);
        return result.Success;
    }

    public async Task<(bool Success, string? Error, Product? CreatedProduct)> CreateProductWithDetailsAsync(Product product)
    {
        // Compatibilidad: crear sin fotos sigue por JSON (Product_id siempre 0, lo asigna BD)
        product.Product_id = 0;
        product.Images = new();
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var payload = ToCreateRequest(product);
            var res = await client.PostAsJsonAsync("api/products", payload);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"CreateProduct failed: {res.StatusCode} - {body}");
                return (false, string.IsNullOrWhiteSpace(body) ? res.ReasonPhrase : body, null);
            }

            Product? created = null;
            try
            {
                var json = await res.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    // La API responde { success, message, data: { product } }
                    // Compatibilidad: si no viene wrapper, intentar deserializar como Product directo
                    var wrapper = System.Text.Json.JsonSerializer.Deserialize<ServiceResponse<Product>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (wrapper?.Data is not null)
                    {
                        created = wrapper.Data;
                    }
                    else
                    {
                        created = System.Text.Json.JsonSerializer.Deserialize<Product>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
            }
            catch
            {
                // La respuesta podría ser vacía o no coincidir con Product
            }

            return (true, null, created);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateProduct exception: {ex}");
            return (false, ex.Message, null);
        }
    }

    public sealed class ProductImageUpload
    {
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int Slot { get; set; }
    }

    public async Task<(bool Success, string? Error, Product? CreatedProduct)> CreateProductWithFilesAsync(Product product, List<ProductImageUpload> files)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            using var content = new MultipartFormDataContent();

            // Producto como JSON (sin Product_id ni Images: BD asigna ID, archivos van aparte)
            product.Product_id = 0;
            var productForSend = new Product
            {
                Product_id = 0,
                Product_Code = product.Product_Code,
                Product_Name = product.Product_Name,
                Product_Type = product.Product_Type,
                Price = product.Price,
                Codebar = product.Codebar,
                Unidad = product.Unidad,
                Peso = product.Peso,
                Costo = product.Costo,
                Stock = product.Stock,
                Stock_Mix = product.Stock_Mix,
                Stock_Max = product.Stock_Max,
                FechaVencimiento = product.FechaVencimiento,
                ManejaVencimiento = product.ManejaVencimiento,
                ManejaSeriales = product.ManejaSeriales,
                ManejaLotes = product.ManejaLotes,
                Description = product.Description,
                SkuNumber = product.SkuNumber,
                PartNumber = product.PartNumber,
                Model = product.Model,
                Marca = product.Marca,
                StatusProducts = product.StatusProducts,
                IsActive = product.IsActive,
                Images = new()
            };
            var productJson = JsonSerializer.Serialize(productForSend, jsonOptions);
            var productContent = new StringContent(productJson, Encoding.UTF8, "application/json");
            content.Add(productContent, "product");

            foreach (var file in files.OrderBy(f => f.Slot))
            {
                if (file?.Data is null || file.Data.Length == 0) continue;
                var fileContent = new ByteArrayContent(file.Data);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
                content.Add(fileContent, "files", string.IsNullOrWhiteSpace(file.FileName) ? $"image_{file.Slot + 1}.jpg" : file.FileName);
            }

            var res = await client.PostAsync("api/products/createproductwithfiles", content);
            var responseJson = await res.Content.ReadAsStringAsync();
            ServiceResponse<Product>? serviceResponse = null;
            try
            {
                serviceResponse = JsonSerializer.Deserialize<ServiceResponse<Product>>(responseJson, jsonOptions);
            }
            catch { }

            if (!res.IsSuccessStatusCode)
            {
                var message = serviceResponse?.Message ?? $"El servidor devolvió el estado {(int)res.StatusCode}.";
                Console.WriteLine($"CreateProductWithFiles failed: {res.StatusCode} - {responseJson}");
                return (false, message, serviceResponse?.Data);
            }

            return (serviceResponse?.Success ?? true, serviceResponse?.Message, serviceResponse?.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateProductWithFiles exception: {ex}");
            return (false, $"Error al crear el producto: {ex.Message}", null);
        }
    }

    public async Task<bool> UpdateProductAsync(int id, Product product)
    {
        var result = await UpdateProductWithDetailsAsync(id, product);
        return result.Success;
    }

    public async Task<(bool Success, string? Error)> UpdateProductWithDetailsAsync(int id, Product product)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var payload = ToUpdateRequest(product);
            var res = await client.PutAsJsonAsync($"api/products/{id}", payload);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"UpdateProduct failed: {res.StatusCode} - {body}");
                return (false, string.IsNullOrWhiteSpace(body) ? res.ReasonPhrase : body);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateProduct exception: {ex}");
            return (false, ex.Message);
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.DeleteAsync($"api/products/{id}");
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static ProductCreateRequest ToCreateRequest(Product product) => new()
    {
        // Product_id es IDENTITY: siempre 0 al crear (evita SQL 544)
        Product_id = 0,
        Product_Code = !string.IsNullOrWhiteSpace(product.Product_Code)
            ? product.Product_Code.Trim()
            : (product.Product_id > 0 ? product.Product_id.ToString() : string.Empty),
        Product_Name = product.Product_Name?.Trim() ?? string.Empty,
        Product_Type = product.Product_Type?.Trim() ?? string.Empty,
        Price = product.Price,
        Codebar = product.Codebar?.Trim() ?? string.Empty,
        Unidad = product.Unidad?.Trim() ?? string.Empty,
        Peso = string.IsNullOrWhiteSpace(product.Peso) ? null : product.Peso.Trim(),
        Costo = product.Costo,
        Stock = product.Stock,
        Stock_Mix = product.Stock_Mix,
        Stock_Max = product.Stock_Max,
        FechaVencimiento = product.FechaVencimiento,
        ManejaVencimiento = product.ManejaVencimiento,
        ManejaSeriales = product.ManejaSeriales,
        ManejaLotes = product.ManejaLotes,
        Description = product.Description,
        SkuNumber = product.SkuNumber,
        PartNumber = product.PartNumber,
        Model = product.Model,
        Marca = product.Marca,
        StatusProducts = product.StatusProducts,
        IsActive = product.IsActive,
        Images = new()
    };

    private static ProductUpdateRequest ToUpdateRequest(Product product) => new()
    {
        Product_id = product.Product_id,
        Product_Code = !string.IsNullOrWhiteSpace(product.Product_Code)
            ? product.Product_Code.Trim()
            : (product.Product_id > 0 ? product.Product_id.ToString() : string.Empty),
        Product_Name = product.Product_Name?.Trim() ?? string.Empty,
        Product_Type = product.Product_Type?.Trim() ?? string.Empty,
        Price = product.Price,
        Codebar = product.Codebar?.Trim() ?? string.Empty,
        Unidad = product.Unidad?.Trim() ?? string.Empty,
        Peso = string.IsNullOrWhiteSpace(product.Peso) ? null : product.Peso.Trim(),
        Costo = product.Costo,
        Stock = product.Stock,
        Stock_Mix = product.Stock_Mix,
        Stock_Max = product.Stock_Max,
        FechaVencimiento = product.FechaVencimiento,
        ManejaVencimiento = product.ManejaVencimiento,
        ManejaSeriales = product.ManejaSeriales,
        ManejaLotes = product.ManejaLotes,
        Description = product.Description,
        SkuNumber = product.SkuNumber,
        PartNumber = product.PartNumber,
        Model = product.Model,
        Marca = product.Marca,
        StatusProducts = product.StatusProducts,
        IsActive = product.IsActive,
        Images = new()
    };

    public string GetImageUrl(int imageId, int productId)
    {
        var client = factory.CreateClient("API");
        var baseUrl = client.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5220";
        return $"{baseUrl}/api/products/getproductimage/{imageId}";
    }

    public class Product
    {
        public int Product_id { get; set; }
        public string Product_Code { get; set; } = "";
        public string Product_Name { get; set; } = "";
        public string Product_Type { get; set; } = "";
        public double Price { get; set; }
        public string Codebar { get; set; } = "";
        public string Unidad { get; set; } = "";
        public string? Peso { get; set; }
        public decimal Costo { get; set; }
        public double Stock { get; set; }
        public double Stock_Mix { get; set; }
        public double Stock_Max { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public bool ManejaVencimiento { get; set; }
        public bool ManejaSeriales { get; set; }
        public bool ManejaLotes { get; set; }
        public string? StockStatus { get; set; } = "";
        public string? Description { get; set; } = "";
        public string? SkuNumber { get; set; } = "";
        public string? PartNumber { get; set; } = "";
        public string? Model { get; set; } = "";
        public string? Marca { get; set; } = "";
        public string? StatusProducts { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaCreado { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByUser { get; set; }
        public string? CreatedByName { get; set; }
        public string? UsuarioCreacion { get; set; }
        public string? UsuarioCreado { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateModified { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? LastModified { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string? UpdatedBy { get; set; }
        public string? UpdatedByUser { get; set; }
        public string? UpdatedByName { get; set; }
        public string? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public string? UsuarioModificacion { get; set; }
        public string? UsuarioActualizacion { get; set; }
        public List<ProductImage> Images { get; set; } = new();
    }

    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? FileName { get; set; }
        public string? StoredFileName { get; set; }
        public string? ContentType { get; set; }
        public int ImageIndex { get; set; }
        public string? Url { get; set; }
    }

    public class ProductCreateRequest
    {
        public int Product_id { get; set; }
        public string Product_Code { get; set; } = "";
        public string Product_Name { get; set; } = "";
        public string Product_Type { get; set; } = "";
        public double Price { get; set; }
        public string Codebar { get; set; } = "";
        public string Unidad { get; set; } = "";
        public string? Peso { get; set; }
        public decimal Costo { get; set; }
        public double Stock { get; set; }
        public double Stock_Mix { get; set; }
        public double Stock_Max { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public bool ManejaVencimiento { get; set; }
        public bool ManejaSeriales { get; set; }
        public bool ManejaLotes { get; set; }
        public string? Description { get; set; }
        public string? SkuNumber { get; set; }
        public string? PartNumber { get; set; }
        public string? Model { get; set; }
        public string? Marca { get; set; }
        public string? StatusProducts { get; set; }
        public bool IsActive { get; set; } = true;
        public List<ProductImage> Images { get; set; } = new();
    }

    public class ProductUpdateRequest : ProductCreateRequest
    {
    }

    public async Task<bool> SetProductActiveAsync(int id, bool active)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.PostAsJsonAsync($"api/products/setactive/{id}", active);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AddProductImageAsync(int productId, string base64Data, string fileName, string contentType, int imageIndex)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var payload = new { Base64Data = base64Data, FileName = fileName, ContentType = contentType };
            var res = await client.PostAsJsonAsync($"api/products/addproductimage/{productId}?imageIndex={imageIndex}", payload);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteProductImageAsync(int imageId)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.DeleteAsync($"api/products/deleteproductimage/{imageId}");
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateProductImageAsync(int imageId, string base64Data, string fileName, string contentType)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var payload = new { Base64Data = base64Data, FileName = fileName, ContentType = contentType };
            var res = await client.PutAsJsonAsync($"api/products/updateproductimage/{imageId}", payload);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<byte[]?> GetProductImageBytesAsync(int imageId)
    {
        try
        {
            var client = await CreateAuthorizedClientAsync();
            var res = await client.GetAsync($"api/products/getproductimage/{imageId}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadAsByteArrayAsync();
        }
        catch
        {
            return null;
        }
    }

    private class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
