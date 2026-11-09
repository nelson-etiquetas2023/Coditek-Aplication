using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Enterprises;

public class EnterpriseService(AppDbContext context) : IEnterprisesService
{
    private readonly AppDbContext context = context;

    public async Task<Enterprise?> GetEnterpriseAsync()
    {
        return await context.Enterprises.FirstOrDefaultAsync();
    }

    public async Task<List<Enterprise>> GetAllAsync()
    {
        return await context.Enterprises
            .OrderBy(e => e.enterprise_id)
            .ToListAsync();
    }

    public async Task<Enterprise> CreateEnterpriseAsync(Enterprise enterprise)
    {
        // Modo estricto: una sola empresa. Si ya existe, actualiza ese registro (upsert).
        var existing = await context.Enterprises.FirstOrDefaultAsync();
        if (existing is not null)
        {
            Apply(enterprise, existing);
            await context.SaveChangesAsync();
            return existing;
        }

        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();
        return enterprise;
    }

    public async Task<Enterprise?> UpdateEnterpriseAsync(int enterpriseId, Enterprise enterprise)
    {
        // Modo estricto: una sola empresa. Actualiza el registro existente.
        var existing = await context.Enterprises.FirstOrDefaultAsync(e => e.enterprise_id == enterpriseId)
            ?? await context.Enterprises.FirstOrDefaultAsync();
        if (existing == null) return null;

        Apply(enterprise, existing);
        await context.SaveChangesAsync();
        return existing;
    }

    private static void Apply(Enterprise source, Enterprise target)
    {
        target.Logo = source.Logo;
        target.LogoContentType = source.LogoContentType;
        target.Codigo_Empresa = source.Codigo_Empresa;
        target.Nombre_Empresa = source.Nombre_Empresa;
        target.Tipo_Empresa = source.Tipo_Empresa;
        target.Registro_Fiscal = source.Registro_Fiscal;
        target.Direccion = source.Direccion;
        target.Telefono = source.Telefono;
        target.Correo = source.Correo;
        target.Latitud = source.Latitud;
        target.Longitud = source.Longitud;
        target.Persona_Contacto = source.Persona_Contacto;
    }
}