using Microsoft.EntityFrameworkCore;
using POSPRA.Domain.Entities;

namespace POSPRA.Infrastructure.Context
{
    public class SqlServerDbContext : DbContext
    {
        //public DbSet<SomeEntity> SomeEntities { get; set; } = null!;
        public DbSet<POSClients> POSClients { get; set; } = null!;

        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Fluent API config if needed
        }
    }
}
