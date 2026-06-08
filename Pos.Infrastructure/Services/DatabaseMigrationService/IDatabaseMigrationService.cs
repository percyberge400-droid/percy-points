namespace Pos.Infrastructure.Services.DatabaseMigrationService
{
    /// <summary>
    /// Compares the live SQLite database schema against the DbContext model
    /// and creates any missing tables or columns.
    /// Nothing is ever dropped or renamed.
    /// </summary>
    public interface IDatabaseMigrationService
    {
        /// <param name="connectionString">
        /// Full SQLite connection string, e.g.
        /// "Data Source=C:\data\pos.db;Mode=ReadWriteCreate;Password=secret"
        /// </param>
        Task ApplyMigrationsAsync();
    }
}
