using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Config;

public class ConfigService(AppDbContext db) : IConfigService
{
    public async Task<List<Category>> GetCategoriesAsync() =>
        await db.Categories.OrderBy(c => c.Name).ToListAsync();

    public async Task<Category?> CreateCategoryAsync(Category category)
    {
        if (category is null || string.IsNullOrWhiteSpace(category.Name)) return null;
        var name = category.Name.Trim();
        if (await db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower())) return null;
        category.Name = name;
        category.Description = category.Description?.Trim() ?? string.Empty;
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(int id, Category category)
    {
        if (category is null || string.IsNullOrWhiteSpace(category.Name)) return null;
        var existing = await db.Categories.FindAsync(id);
        if (existing is null) return null;
        var name = category.Name.Trim();
        if (await db.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower())) return null;
        existing.Name = name;
        existing.Description = category.Description?.Trim() ?? string.Empty;
        existing.IsActive = category.IsActive;
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return false;
        db.Categories.Remove(c);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ProductUnit>> GetProductUnitsAsync() =>
        await db.ProductUnits.OrderBy(u => u.Name).ToListAsync();

    public async Task<ProductUnit?> CreateProductUnitAsync(ProductUnit unit)
    {
        if (unit is null || string.IsNullOrWhiteSpace(unit.Name)) return null;
        var name = unit.Name.Trim();
        if (await db.ProductUnits.AnyAsync(u => u.Name.ToLower() == name.ToLower())) return null;
        unit.Name = name;
        unit.Description = unit.Description?.Trim() ?? string.Empty;
        db.ProductUnits.Add(unit);
        await db.SaveChangesAsync();
        return unit;
    }

    public async Task<ProductUnit?> UpdateProductUnitAsync(int id, ProductUnit unit)
    {
        if (unit is null || string.IsNullOrWhiteSpace(unit.Name)) return null;
        var existing = await db.ProductUnits.FindAsync(id);
        if (existing is null) return null;
        var name = unit.Name.Trim();
        if (await db.ProductUnits.AnyAsync(u => u.Id != id && u.Name.ToLower() == name.ToLower())) return null;
        existing.Name = name;
        existing.Description = unit.Description?.Trim() ?? string.Empty;
        existing.IsActive = unit.IsActive;
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteProductUnitAsync(int id)
    {
        var u = await db.ProductUnits.FindAsync(id);
        if (u is null) return false;
        db.ProductUnits.Remove(u);
        await db.SaveChangesAsync();
        return true;
    }
}