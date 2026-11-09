using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class ProductAudit
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }

    public DateTime ChangedAt { get; set; }

    [StringLength(256)]
    public string? ChangedBy { get; set; }

    [StringLength(50)]
    public string Action { get; set; } = "Updated";

    /// <summary>Resumen de cambios (campos modificados).</summary>
    public string? Changes { get; set; }
}
