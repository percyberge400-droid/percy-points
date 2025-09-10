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

        private static readonly string DbPath;

        static SqliteDbContext()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "POSPRA");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            DbPath = Path.Combine(folder, "pospra.db");
        }

        public SqliteDbContext(DbContextOptions<SqliteDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={DbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "admin123" }
            );

            modelBuilder.Entity<Invoice>().HasNoKey();
        }

        public static string GetDbPath() => DbPath;
    }
}
