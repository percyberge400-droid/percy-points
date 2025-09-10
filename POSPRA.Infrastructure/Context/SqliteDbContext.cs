using Microsoft.EntityFrameworkCore;
using POSPRA.Domain.Entities;

namespace POSPRA.Infrastructure.Context
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<FileRecord> FileRecords { get; set; } = null!;
        public DbSet<Logs> Logs { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;


        private readonly string _dbPath;

        // Constructor for Dependency Injection (API)
        public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
        {
        }

        // Constructor for manual usage (WinForms)
        public SqliteDbContext()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "POSPRA");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            _dbPath = Path.Combine(folder, "pospra.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_dbPath))
            {
                optionsBuilder.UseSqlite($"Data Source={_dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "admin123" }
            );

            modelBuilder.Entity<Invoice>().HasNoKey();
        }
    }
}