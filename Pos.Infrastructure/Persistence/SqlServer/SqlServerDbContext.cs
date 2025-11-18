using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Pos.Domain.Entities;

public class SqlServerDbContext : DbContext
{
    public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<PosClients> PosClients { get; set; } = null!;
    public DbSet<Invoice> Invoice { get; set; } = null!;
    public DbSet<InvoiceItems> InvoiceItems { get; set; } = null!;
    public DbSet<POSConfigurations> POSConfigurations { get; set; } = null!;
    public DbSet<PosStatus> POSStatus { get; set; }
    public DbSet<ProductCatalogue> ProductCatalogue { get; set; } = null!;
    public DbSet<Logs> Logs { get; set; } = null!;
    public DbSet<POSBranches> POSBranches { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PosClients>(entity =>
        {
            entity.ToTable("PosClients");
            entity.Property(e => e.POSRegistrationNumber).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(150).IsRequired();
        });
    }
}
