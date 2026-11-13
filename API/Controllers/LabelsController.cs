using API.Models;
using API.Services.Labels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LabelsController(ILabelsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LabelTemplate>>> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LabelTemplate>> Get(int id)
    {
        var item = await service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("next-code")]
    public async Task<ActionResult<object>> NextCode() =>
        Ok(new { Codigo = await service.GetNextCodigoAsync() });

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LabelTemplate>> Create([FromBody] LabelTemplate label)
    {
        if (string.IsNullOrWhiteSpace(label.Descripcion))
            return BadRequest("La descripción de la etiqueta es obligatoria.");
        var created = await service.CreateAsync(label);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LabelTemplate>> Update(int id, [FromBody] LabelTemplate label)
    {
        if (string.IsNullOrWhiteSpace(label.Descripcion))
            return BadRequest("La descripción de la etiqueta es obligatoria.");
        var updated = await service.UpdateAsync(id, label);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await service.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    public record RenderRequest(string Zpl, decimal WidthMm, decimal LengthMm, int Dpmm = 8, int Index = 0);

    [HttpPost("render")]
    [AllowAnonymous] // preview; tighten if needed
    public async Task<IActionResult> Render([FromBody] RenderRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Zpl))
            return BadRequest("ZPL vacío.");

        var dpmm = req.Dpmm is 6 or 8 or 12 or 24 ? req.Dpmm : 8;
        var widthIn = (req.WidthMm / 25.4m);
        var lengthIn = (req.LengthMm / 25.4m);
        if (widthIn <= 0 || lengthIn <= 0) return BadRequest("Dimensiones inválidas.");

        var url = string.Format(CultureInfo.InvariantCulture,
            "https://api.labelary.com/v1/printers/{0}dpmm/labels/{1}x{2}/{3}/",
            dpmm,
            Math.Round(widthIn, 4),
            Math.Round(lengthIn, 4),
            Math.Max(0, req.Index));

        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png"));
            using var content = new StringContent(req.Zpl, Encoding.UTF8, "application/x-www-form-urlencoded");
            var resp = await http.PostAsync(url, content);
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            if (!resp.IsSuccessStatusCode)
            {
                var msg = Encoding.UTF8.GetString(bytes);
                return StatusCode((int)resp.StatusCode, string.IsNullOrWhiteSpace(msg) ? "Error al renderizar ZPL." : msg);
            }
            return File(bytes, "image/png");
        }
        catch (Exception ex)
        {
            return StatusCode(502, $"Error al contactar Labelary: {ex.Message}");
        }
    }
}
