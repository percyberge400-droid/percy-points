using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Infrastructure.Persistence.Repositories;

namespace Pos.Infrastructure.Persistence.Factory
{
    public class SqliteDynamicFactory : ISqliteDynamicFactory
    {
        /// <summary>
        /// Create repository and unit of work for encrypted SQLite database
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="dbPath">Full path to SQLite DB file</param>
        /// <param name="password">Encryption password for SQLCipher</param>
        /// <returns>Tuple of repository and unit of work</returns>
        public (IRepository<T> repo, IUnitOfWork uow) Create<T>(string dbPath, string password) where T : class
        {
            // Ensure the folder exists
            var folder = System.IO.Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(folder))
                System.IO.Directory.CreateDirectory(folder);

            // Build connection string with password for SQLCipher
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = password
            }.ToString();

            // Create and open connection (key applied here)
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Build EF Core options using the open connection
            var options = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite(connection)
                .Options;

            // Create DbContext
            var context = new SqliteDbContext(options);

            // Create unit of work and repository
            var uow = new SqliteUnitOfWork(context);
            var repo = new Repository<T>(context);

            return (repo, uow);
        }
    }
}
