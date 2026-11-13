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

    public async Task<LabelTemplate> CreateAsync(LabelTemplate label)
    {
        label.Id = 0;
        label.Codigo = string.Empty; // se asigna tras el INSERT a partir del Id
        context.LabelTemplates.Add(label);
        await context.SaveChangesAsync();
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
