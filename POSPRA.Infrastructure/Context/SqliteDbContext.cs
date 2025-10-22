using Microsoft.EntityFrameworkCore;
using POSPRA.Domain.Entities;

namespace POSPRA.Infrastructure.Context
{
    /// <summary>
    /// EF Core DbContext for the SQLite database used by POSPRA.
    /// Provides DbSet properties for all main entities like Users, FileRecords, Logs, and Invoices.
    /// The database file will be created in the same folder as the application executable.
    /// </summary>
    public class SqliteDbContext : DbContext
    {
        /// <summary>Users table.</summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>FileRecords table.</summary>
        public DbSet<FileRecord> FileRecords { get; set; } = null!;

        /// <summary>Product Catalog Table.</summary>
        public DbSet<ProductCatalogue> ProductCatalogue { get; set; } = null!;

        /// <summary>Logs table.</summary>
        public DbSet<Logs> Logs { get; set; } = null!;

        // 🔹 Path to SQLite DB file
        private static string? _dbPath;

        public static void SetDatabasePath(string path)
        {
            _dbPath = path;
        }

        public static string GetDbPath()
        {
            return _dbPath ?? Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
        }

        public SqliteDbContext(DbContextOptions<SqliteDbContext> options)
            : base(options) { }

        public SqliteDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={GetDbPath()}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed default admin user
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "admin123" }
            );
        }

        /// <summary>
        /// Returns the full path to the SQLite database file.
        /// </summary>
    }
}
