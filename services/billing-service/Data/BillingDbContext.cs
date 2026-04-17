using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>("invoice_numbers")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");
            entity.HasKey(invoice => invoice.Id);

            entity.Property(invoice => invoice.Number)
                .HasDefaultValueSql("nextval('invoice_numbers')")
                .ValueGeneratedOnAdd();

            entity.HasIndex(invoice => invoice.Number)
                .IsUnique();

            entity.Property(invoice => invoice.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(invoice => invoice.CreatedAtUtc)
                .IsRequired();

            entity.HasMany(invoice => invoice.Items)
                .WithOne(item => item.Invoice)
                .HasForeignKey(item => item.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("invoice_items");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.ProductCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(item => item.ProductDescription)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(item => item.Quantity)
                .IsRequired();
        });
    }
}
