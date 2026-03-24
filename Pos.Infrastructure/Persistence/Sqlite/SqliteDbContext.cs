using Microsoft.EntityFrameworkCore;
using Pos.Domain.Entities;

public class SqliteDbContext : DbContext
{
    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options) { }

    /// <summary>FileRecords table.</summary>
    public DbSet<FileRecord> FileRecords { get; set; } = null!;

    /// <summary>Product Catalog Table.</summary>
    public DbSet<ProductCatalogue> ProductCatalogue { get; set; } = null!;
    public DbSet<Payment> Payment { get; set; } = null!;

    /// <summary>Logs table.</summary>
    public DbSet<Logs> Logs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PosClients>(entity =>
        {
            entity.ToTable("PosClients");
            entity.Property(e => e.POSRegistrationNumber).IsRequired();
        });
    }
}
