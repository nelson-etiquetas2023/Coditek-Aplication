using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class Product
{
    [Key]
    public int Product_id { get; set; }

    [Required]
    public string Product_Code { get; set; } = "";

    [Required]
    public string Product_Name { get; set; } = null!;

    [Required]
    public string Product_Type { get; set; } = null!;

    public double Price { get; set; } = 0;

    public string Codebar { get; set; } = null!;

    [Required]
    public string Unidad { get; set; } = null!;

    public decimal Costo { get; set; } = 0;
    public double Stock { get; set; } = 0;
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
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public List<ProductImage> Images { get; set; } = new();
}

public class ProductImage
{
    [Key]
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? FileName { get; set; }
    public string? StoredFileName { get; set; }
    public string? ContentType { get; set; }
    public int ImageIndex { get; set; }
    public string? Url { get; set; }
}

public class ProductUnit
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

