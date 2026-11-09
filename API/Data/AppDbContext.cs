using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Product> Productos { get; set; }
    public DbSet<ProductUnit> ProductUnits { get; set; }
    public DbSet<ProductImage> Images { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Enterprise> Enterprises { get; set; }
    public DbSet<LabelTemplate> LabelTemplates { get; set; }
    public DbSet<ProductAudit> ProductAudits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.Costo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<User>()
            .Property(u => u.Nickname)
            .HasMaxLength(50);

        modelBuilder.Entity<LabelTemplate>()
            .Property(l => l.WidthMm)
            .HasPrecision(10, 2);
        modelBuilder.Entity<LabelTemplate>()
            .Property(l => l.LengthMm)
            .HasPrecision(10, 2);

        modelBuilder.Entity<ProductAudit>()
            .HasIndex(a => a.ProductId);
        modelBuilder.Entity<ProductAudit>()
            .Property(a => a.ChangedBy)
            .HasMaxLength(256);
        modelBuilder.Entity<ProductAudit>()
            .Property(a => a.Action)
            .HasMaxLength(50);
    }
}
