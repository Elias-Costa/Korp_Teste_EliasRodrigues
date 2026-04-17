using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(product => product.Id);

            entity.Property(product => product.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(product => product.Code)
                .IsUnique();

            entity.Property(product => product.Description)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(product => product.Stock)
                .IsRequired();

            entity.Property(product => product.CreatedAtUtc)
                .IsRequired();
        });
    }
}
