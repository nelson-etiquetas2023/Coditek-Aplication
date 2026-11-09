namespace API.Models;

public class Base64ImageData
{
    public string? Base64Data { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
}

public class CreateProductWithImagesRequest
{
    public Product? Product { get; set; }
    public List<Base64ImageData> Images { get; set; } = new();
}