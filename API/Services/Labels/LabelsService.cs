using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Labels;

public class LabelsService(AppDbContext context) : ILabelsService
{
    public async Task<List<LabelTemplate>> GetAllAsync() =>
        await context.LabelTemplates.OrderBy(l => l.Codigo).ThenBy(l => l.Id).ToListAsync();

    public async Task<LabelTemplate?> GetByIdAsync(int id) =>
        await context.LabelTemplates.FindAsync(id);

    public async Task<string> GetNextCodigoAsync()
    {
        var maxId = await context.LabelTemplates.MaxAsync(l => (int?)l.Id) ?? 0;
        return $"ETI-{maxId + 1:D4}";
    }

    public async Task<LabelTemplate> CreateAsync(LabelTemplate label)
    {
        var requested = label.Codigo?.Trim() ?? string.Empty;
        label.Id = 0;
        label.Codigo = string.Empty; // se asigna tras el INSERT
        context.LabelTemplates.Add(label);
        await context.SaveChangesAsync();

        if (requested.Length > 0 && !await context.LabelTemplates.AnyAsync(l => l.Codigo == requested))
        {
            label.Codigo = requested;
            try
            {
                await context.SaveChangesAsync();
                return label;
            }
            catch (DbUpdateException)
            {
                // Otra creación simultánea tomó el código solicitado: usar el basado en Id.
            }
        }

        label.Codigo = $"ETI-{label.Id:D4}";
        await context.SaveChangesAsync();
        return label;
    }

    public async Task<LabelTemplate?> UpdateAsync(int id, LabelTemplate label)
    {
        var existing = await context.LabelTemplates.FindAsync(id);
        if (existing is null) return null;

        existing.Descripcion = label.Descripcion;
        existing.WidthMm = label.WidthMm;
        existing.LengthMm = label.LengthMm;
        existing.CantidadPorRollo = label.CantidadPorRollo;
        existing.CodigoZpl = label.CodigoZpl;
        existing.Dpmm = label.Dpmm > 0 ? label.Dpmm : existing.Dpmm;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await context.LabelTemplates.FindAsync(id);
        if (existing is null) return false;
        context.LabelTemplates.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}
