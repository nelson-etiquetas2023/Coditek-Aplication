using API.Data;
using API.Models;
using API.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace API.Services.Products;

public interface IProductsService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<ServiceResponse<Product>> CreateAsync(Product product, string? userName = null);
    Task<ServiceResponse<Product>> UpdateAsync(int id, Product product, string? userName = null);
    Task<bool> DeleteAsync(int id);
    Task<ServiceResponse<Product>> CreateWithImagesAsync(CreateProductWithImagesRequest request, string? userName = null);
    Task<ServiceResponse<Product>> CreateWithFilesAsync(Product product, Microsoft.AspNetCore.Http.IFormFileCollection files, string? userName = null);
    Task<bool> SetProductActiveAsync(int id, bool active, string? userName = null);
    Task<bool> AddProductImageAsync(int productId, string base64Data, string fileName, string contentType, int imageIndex);
    Task<bool> DeleteProductImageAsync(int imageId);
    Task<bool> UpdateProductImageAsync(int imageId, string base64Data, string fileName, string contentType);
}

public class ProductsService(AppDbContext db, IWebHostEnvironment env, IConfiguration config) : IProductsService
{
    public async Task<List<Product>> GetAllAsync() =>
        await db.Productos.Include(p => p.Images).ToListAsync();

    public async Task<Product?> GetByIdAsync(int id) =>
        await db.Productos.Include(p => p.Images).FirstOrDefaultAsync(p => p.Product_id == id);

    public async Task<ServiceResponse<Product>> CreateAsync(Product product, string? userName = null)
    {
        var code = product.Product_Code?.Trim() ?? string.Empty;

        // Commented out code validation to allow product creation without mandatory code
        // Original validation:
        // if (string.IsNullOrWhiteSpace(code))
        //     return new ServiceResponse<Product> { Success = false, Message = "El codigo de producto es obligatorio." };
        //
        // var exists = await db.Productos.AnyAsync(p => p.Product_Code == code);
        // if (exists)
        //     return new ServiceResponse<Product> { Success = false, Message = $"El codigo {code} ya existe." };

        product.Product_Code = code;
        // Product_id es IDENTITY en BD: nunca insertar explícito (evita SQL 544)
        product.Product_id = 0;
        product.Images = new();
        var now = DateTime.Now;
        product.CreatedAt = now;
        product.CreatedBy = userName;
        product.UpdatedAt = now;
        product.UpdatedBy = userName;
        db.Productos.Add(product);
        await db.SaveChangesAsync();
        return new ServiceResponse<Product> { Success = true, Message = "Producto creado.", Data = product };
    }

    public async Task<ServiceResponse<Product>> UpdateAsync(int id, Product product, string? userName = null)
    {
        var existing = await db.Productos.FindAsync(id);
        if (existing is null)
            return new ServiceResponse<Product> { Success = false, Message = "Producto no encontrado." };

        var now = DateTime.Now;
        var changes = new List<string>();
        void Diff(string field, object? oldVal, object? newVal)
        {
            var o = oldVal?.ToString() ?? "";
            var n = newVal?.ToString() ?? "";
            if (!string.Equals(o, n, StringComparison.Ordinal)) changes.Add($"{field}: '{o}' -> '{n}'");
        }
        Diff("Product_Code", existing.Product_Code, product.Product_Code);
        Diff("Product_Name", existing.Product_Name, product.Product_Name);
        Diff("Product_Type", existing.Product_Type, product.Product_Type);
        Diff("Price", existing.Price, product.Price);
        Diff("Codebar", existing.Codebar, product.Codebar);
        Diff("Unidad", existing.Unidad, product.Unidad);
        Diff("Costo", existing.Costo, product.Costo);
        Diff("Stock", existing.Stock, product.Stock);
        Diff("Stock_Mix", existing.Stock_Mix, product.Stock_Mix);
        Diff("Stock_Max", existing.Stock_Max, product.Stock_Max);
        Diff("FechaVencimiento", existing.FechaVencimiento, product.FechaVencimiento);
        Diff("ManejaVencimiento", existing.ManejaVencimiento, product.ManejaVencimiento);
        Diff("ManejaSeriales", existing.ManejaSeriales, product.ManejaSeriales);
        Diff("ManejaLotes", existing.ManejaLotes, product.ManejaLotes);
        Diff("Description", existing.Description, product.Description);
        Diff("SkuNumber", existing.SkuNumber, product.SkuNumber);
        Diff("PartNumber", existing.PartNumber, product.PartNumber);
        Diff("Model", existing.Model, product.Model);
        Diff("Marca", existing.Marca, product.Marca);
        Diff("StatusProducts", existing.StatusProducts, product.StatusProducts);
        Diff("IsActive", existing.IsActive, product.IsActive);

        existing.Product_Code = product.Product_Code;
        existing.Product_Name = product.Product_Name;
        existing.Product_Type = product.Product_Type;
        existing.Price = product.Price;
        existing.Codebar = product.Codebar;
        existing.Unidad = product.Unidad;
        existing.Costo = product.Costo;
        existing.Stock = product.Stock;
        existing.Stock_Mix = product.Stock_Mix;
        existing.Stock_Max = product.Stock_Max;
        existing.FechaVencimiento = product.FechaVencimiento;
        existing.ManejaVencimiento = product.ManejaVencimiento;
        existing.ManejaSeriales = product.ManejaSeriales;
        existing.ManejaLotes = product.ManejaLotes;
        existing.Description = product.Description;
        existing.SkuNumber = product.SkuNumber;
        existing.PartNumber = product.PartNumber;
        existing.Model = product.Model;
        existing.Marca = product.Marca;
        existing.StatusProducts = product.StatusProducts;
        existing.IsActive = product.IsActive;
        existing.UpdatedAt = now;
        existing.UpdatedBy = userName;

        db.ProductAudits.Add(new ProductAudit
        {
            ProductId = existing.Product_id,
            ChangedAt = now,
            ChangedBy = userName,
            Action = "Updated",
            Changes = changes.Count > 0 ? string.Join("; ", changes) : "(sin cambios detectados)"
        });

        await db.SaveChangesAsync();
        return new ServiceResponse<Product> { Success = true, Message = "Producto actualizado.", Data = existing };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var p = await db.Productos.FindAsync(id);
        if (p is null) return false;
        db.Productos.Remove(p);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<ServiceResponse<Product>> CreateWithImagesAsync(CreateProductWithImagesRequest request, string? userName = null)
    {
        if (request.Product is null)
            return new ServiceResponse<Product> { Success = false, Message = "Datos del producto invalidos." };

        var code = request.Product.Product_Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
            return new ServiceResponse<Product> { Success = false, Message = "El codigo de producto es obligatorio." };

        var exists = await db.Productos.AnyAsync(p => p.Product_Code == code);
        if (exists)
            return new ServiceResponse<Product> { Success = false, Message = $"El codigo {code} ya existe." };

        request.Product.Product_Code = code;
        // Identity: no insertar Product_id explícito (SQL 544) ni Images Url fantasma
        request.Product.Product_id = 0;
        request.Product.Images = new();
        var now = DateTime.Now;
        request.Product.CreatedAt = now;
        request.Product.CreatedBy = userName;
        request.Product.UpdatedAt = now;
        request.Product.UpdatedBy = userName;
        db.Productos.Add(request.Product);
        await db.SaveChangesAsync();

        var imagesPath = ProductImageStorage.GetPath(env, config);
        Directory.CreateDirectory(imagesPath);

        int idx = 0;
        foreach (var img in request.Images)
        {
            if (string.IsNullOrEmpty(img.Base64Data)) continue;
            try
            {
                var base64Index = img.Base64Data.IndexOf(',');
                var cleanBase64 = base64Index >= 0 ? img.Base64Data[(base64Index + 1)..] : img.Base64Data;
                var bytes = Convert.FromBase64String(cleanBase64);
                var ext = Path.GetExtension(img.FileName ?? "").ToLower();
                if (string.IsNullOrEmpty(ext)) ext = ".jpg";
                var storedName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(imagesPath, storedName);
                await File.WriteAllBytesAsync(filePath, bytes);

                var pi = new ProductImage
                {
                    ProductId = request.Product.Product_id,
                    FileName = img.FileName,
                    StoredFileName = storedName,
                    ContentType = img.ContentType ?? "image/jpeg",
                    ImageIndex = idx++
                };
                db.Images.Add(pi);
            }
            catch { }
        }
        await db.SaveChangesAsync();

        var created = await db.Productos.Include(p => p.Images).FirstOrDefaultAsync(p => p.Product_id == request.Product.Product_id);
        return new ServiceResponse<Product> { Success = true, Message = "Producto creado.", Data = created };
    }

    public async Task<bool> SetProductActiveAsync(int id, bool active, string? userName = null)
    {
        var p = await db.Productos.FindAsync(id);
        if (p is null) return false;
        var old = p.IsActive;
        var now = DateTime.Now;
        p.IsActive = active;
        p.UpdatedAt = now;
        p.UpdatedBy = userName;
        db.ProductAudits.Add(new ProductAudit
        {
            ProductId = p.Product_id,
            ChangedAt = now,
            ChangedBy = userName,
            Action = "SetActive",
            Changes = $"IsActive: '{old}' -> '{active}'"
        });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddProductImageAsync(int productId, string base64Data, string fileName, string contentType, int imageIndex)
    {
        var product = await db.Productos.FindAsync(productId);
        if (product is null) return false;
        try
        {
            var base64Index = base64Data.IndexOf(",");
            var cleanBase64 = base64Index >= 0 ? base64Data[(base64Index + 1)..] : base64Data;
            var bytes = Convert.FromBase64String(cleanBase64);
            var ext = Path.GetExtension(fileName ?? "").ToLower();
            if (string.IsNullOrEmpty(ext)) ext = ".jpg";
            var storedName = $"{Guid.NewGuid()}{ext}";
            var imagesPath = ProductImageStorage.GetPath(env, config);
            Directory.CreateDirectory(imagesPath);
            var filePath = Path.Combine(imagesPath, storedName);
            await File.WriteAllBytesAsync(filePath, bytes);

            var pi = new ProductImage
            {
                ProductId = productId,
                FileName = fileName,
                StoredFileName = storedName,
                ContentType = contentType ?? "image/jpeg",
                ImageIndex = imageIndex
            };
            db.Images.Add(pi);
            await db.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteProductImageAsync(int imageId)
    {
        var img = await db.Images.FindAsync(imageId);
        if (img is null) return false;
        try
        {
            var imagesPath = ProductImageStorage.GetPath(env, config);
            var filePath = Path.Combine(imagesPath, img.StoredFileName ?? string.Empty);
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
        }
        catch { }
        db.Images.Remove(img);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateProductImageAsync(int imageId, string base64Data, string fileName, string contentType)
    {
        var img = await db.Images.FindAsync(imageId);
        if (img is null) return false;
        try
        {
            var base64Index = base64Data.IndexOf(",");
            var cleanBase64 = base64Index >= 0 ? base64Data[(base64Index + 1)..] : base64Data;
            var bytes = Convert.FromBase64String(cleanBase64);
            var ext = Path.GetExtension(fileName ?? "").ToLower();
            if (string.IsNullOrEmpty(ext)) ext = ".jpg";
            var storedName = $"{Guid.NewGuid()}{ext}";
            var imagesPath = ProductImageStorage.GetPath(env, config);
            Directory.CreateDirectory(imagesPath);
            var filePath = Path.Combine(imagesPath, storedName);
            await File.WriteAllBytesAsync(filePath, bytes);

            // Eliminar archivo anterior
            var oldPath = Path.Combine(imagesPath, img.StoredFileName ?? string.Empty);
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);

            img.FileName = fileName;
            img.StoredFileName = storedName;
            img.ContentType = contentType ?? "image/jpeg";
            await db.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }

    public async Task<ServiceResponse<Product>> CreateWithFilesAsync(Product product, Microsoft.AspNetCore.Http.IFormFileCollection files, string? userName = null)
    {
        var savedFiles = new List<string>();
        if (product is null)
            return new ServiceResponse<Product> { Success = false, Message = "El producto es obligatorio." };

        var code = product.Product_Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
            return new ServiceResponse<Product> { Success = false, Message = "El codigo de producto es obligatorio." };

        var exists = await db.Productos.AnyAsync(p => p.Product_Code == code);
        if (exists)
            return new ServiceResponse<Product> { Success = false, Message = $"El codigo {code} ya existe." };

        product.Product_Code = code;
        product.Product_Name ??= "";
        product.Product_Type ??= "";
        product.Codebar ??= "";
        product.Unidad ??= "";
        product.Description ??= "";
        product.SkuNumber ??= "";
        product.PartNumber ??= "";
        product.Model ??= "";
        product.Marca ??= "";
        product.StatusProducts ??= "";
        // Identity: dejar que BD asigne (evita SQL 544)
        product.Product_id = 0;
        product.Images = new();
        var now = DateTime.Now;
        product.CreatedAt = now;
        product.CreatedBy = userName;
        product.UpdatedAt = now;
        product.UpdatedBy = userName;

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            db.Productos.Add(product);
            await db.SaveChangesAsync();

            var productId = product.Product_id;
            if (productId <= 0)
                throw new InvalidOperationException("El identificador del producto no fue asignado.");

            var imagesPath = ProductImageStorage.GetPath(env, config);
            Directory.CreateDirectory(imagesPath);
            int count = files.Count;
            if (count > 3) count = 3;
            for (var i = 0; i < count; i++)
            {
                var file = files[i];
                if (file is null || file.Length == 0) continue;

                var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(imagesPath, storedFileName);

                await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }

                savedFiles.Add(filePath);
                db.Images.Add(new ProductImage
                {
                    ProductId = productId,
                    FileName = file.FileName,
                    StoredFileName = storedFileName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    ImageIndex = i
                });
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            var created = await db.Productos.Include(p => p.Images).FirstOrDefaultAsync(p => p.Product_id == productId);
            return new ServiceResponse<Product> { Success = true, Message = "Producto creado.", Data = created };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            foreach (var filePath in savedFiles)
            {
                try { File.Delete(filePath); } catch { }
            }
            return new ServiceResponse<Product> { Success = false, Message = $"Error al guardar el producto: {ex.Message}" };
        }
    }
}
