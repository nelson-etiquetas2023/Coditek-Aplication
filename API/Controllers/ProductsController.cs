using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services.Products;
using API.Data;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductsService service) : ControllerBase
{
    private string? CurrentUserName()
    {
        var name = User.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(name)) return name;
        name = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("name") ?? User.FindFirstValue("unique_name") ?? User.FindFirstValue("nickname") ?? User.FindFirstValue(ClaimTypes.Email);
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll() =>
        await service.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
        var p = await service.GetByIdAsync(id);
        return p is null ? NotFound() : p;
    }

    [HttpPost]
    public async Task<ActionResult> Create(Product product)
    {
        var result = await service.CreateAsync(product, CurrentUserName());
        if (!result.Success) return Conflict(new { result.Success, result.Message });
        return Ok(result);
    }

    [HttpPost("createproductwithfiles")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<ActionResult> CreateWithFiles()
    {
        var form = await Request.ReadFormAsync();
        var productJson = form["product"].FirstOrDefault();
        var files = form.Files;

        if (string.IsNullOrEmpty(productJson))
            return BadRequest("Missing product data");

        Product? product = null;
        try
        {
            product = JsonSerializer.Deserialize<Product>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            return BadRequest($"Invalid product JSON: {ex.Message}");
        }

        if (product is null)
            return BadRequest("Missing product data");

        var result = await service.CreateWithFilesAsync(product, files, CurrentUserName());
        if (!result.Success) return Conflict(new { result.Success, result.Message });
        return Ok(result);
    }

    [HttpPost("createproductwithimages")]
    public async Task<ActionResult> CreateWithImages(CreateProductWithImagesRequest request)
    {
        var result = await service.CreateWithImagesAsync(request, CurrentUserName());
        if (!result.Success) return Conflict(new { result.Success, result.Message });
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Product product)
    {
        var result = await service.UpdateAsync(id, product, CurrentUserName());
        if (!result.Success) return BadRequest(new { result.Success, result.Message });
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await service.DeleteAsync(id)) return NotFound();
        return NoContent();
    }

    [HttpGet("getproductimage/{imageId}")]
    public async Task<IActionResult> GetProductImage(int imageId)
    {
        var context = HttpContext.RequestServices.GetService(typeof(API.Data.AppDbContext)) as API.Data.AppDbContext;
        if (context is null) return StatusCode(500, "Database context not available");

        var image = await context.Images.FindAsync(imageId);
        if (image is null) return NotFound();

        var imagesPath = API.Storage.ProductImageStorage.GetPath(
            HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>(),
            HttpContext.RequestServices.GetRequiredService<IConfiguration>());
        var filePath = Path.Combine(imagesPath, image.StoredFileName ?? string.Empty);
        if (!System.IO.File.Exists(filePath)) return NotFound("File not found on server");

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, image.ContentType ?? "application/octet-stream", image.FileName);
    }
    [HttpPost("setactive/{id:int}")]
    public async Task<IActionResult> SetActive(int id, [FromBody] bool active)
    {
        var ok = await service.SetProductActiveAsync(id, active, CurrentUserName());
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("addproductimage/{productId:int}")]
    public async Task<IActionResult> AddProductImage(int productId, [FromBody] Base64ImageData imageData, [FromQuery] int imageIndex = 0)
    {
        if (imageData is null) return BadRequest("Invalid image data");
        var ok = await service.AddProductImageAsync(productId, imageData.Base64Data ?? string.Empty, imageData.FileName ?? "image.jpg", imageData.ContentType ?? "image/jpeg", imageIndex);
        return ok ? Ok(new { message = "Image added" }) : NotFound("Product not found");
    }

    [HttpDelete("deleteproductimage/{imageId:int}")]
    public async Task<IActionResult> DeleteProductImage(int imageId)
    {
        var ok = await service.DeleteProductImageAsync(imageId);
        return ok ? Ok(new { message = "Image deleted" }) : NotFound("Image not found");
    }

    [HttpPut("updateproductimage/{imageId:int}")]
    public async Task<IActionResult> UpdateProductImage(int imageId, [FromBody] Base64ImageData imageData)
    {
        if (imageData is null) return BadRequest("Invalid image data");
        var ok = await service.UpdateProductImageAsync(imageId, imageData.Base64Data ?? string.Empty, imageData.FileName ?? "image.jpg", imageData.ContentType ?? "image/jpeg");
        return ok ? Ok(new { message = "Image updated" }) : NotFound("Image not found");
    }
}