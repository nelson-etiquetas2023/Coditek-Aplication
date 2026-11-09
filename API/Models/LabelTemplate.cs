using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class LabelTemplate
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la etiqueta es obligatorio.")]
    [MaxLength(200)]
    public string Nombre { get; set; } = "";

    [MaxLength(1000)]
    public string Descripcion { get; set; } = "";

    /// <summary>Ancho de la etiqueta en milímetros.</summary>
    public decimal WidthMm { get; set; }

    /// <summary>Largo/alto de la etiqueta en milímetros.</summary>
    public decimal LengthMm { get; set; }

    /// <summary>Cantidad de etiquetas por rollo.</summary>
    public int CantidadPorRollo { get; set; }

    /// <summary>Código ZPL de la etiqueta.</summary>
    public string CodigoZpl { get; set; } = "";

    /// <summary>Densidad para preview (dpmm: 6,8,12,24). Default 8 = 203dpi.</summary>
    public int Dpmm { get; set; } = 8;
}
