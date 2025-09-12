using Microsoft.EntityFrameworkCore;

namespace POSPRA.Infrastructure.Context
{
    /// <summary>
    /// EF Core DbContext for the SQL Server database used by POSPRA.
    /// <para>
    /// Add DbSet properties for all entities that will be stored in SQL Server.
    /// </para>
    /// </summary>
    public class SqlServerDbContext : DbContext
    {
        // Example DbSets:
        // public DbSet<SomeEntity> SomeEntities { get; set; } = null!;
        // public DbSet<POSClients> POSClients { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of <see cref="SqlServerDbContext"/> with the specified options.
        /// </summary>
        /// <param name="options">The DbContext options to configure the connection.</param>
        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options)
            : base(options) { }

        /// <summary>
        /// Configure the model using Fluent API.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add Fluent API configurations here if needed
            // e.g., modelBuilder.Entity<SomeEntity>().HasKey(e => e.Id);
        }
    }
}
