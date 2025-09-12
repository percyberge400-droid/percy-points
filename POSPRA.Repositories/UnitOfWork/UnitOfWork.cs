using Microsoft.EntityFrameworkCore;
using POSPRA.Infrastructure.Context;

namespace POSPRA.Repositories.UnitOfWork
{
    /// <summary>
    /// Base contract for all Unit of Work implementations.
    /// Provides a single SaveChanges method and proper disposal semantics.
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Persists all pending changes to the underlying database.
        /// Returns the number of affected rows.
        /// </summary>
        Task<int> SaveChangesAsync();
    }

    /// <summary>
    /// Generic Unit of Work implementation that works with any EF Core DbContext.
    /// </summary>
    /// <typeparam name="TContext">
    /// The concrete DbContext type this unit of work manages.
    /// </typeparam>
    public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        private readonly TContext _context;

        /// <summary>
        /// Creates a new UnitOfWork instance for the specified DbContext.
        /// </summary>
        public UnitOfWork(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Unit of Work implementation dedicated to the SQLite database.
    /// Use when working with <see cref="SqliteDbContext"/> repositories.
    /// </summary>
    public class SqliteUnitOfWork : UnitOfWork<SqliteDbContext>, ISqliteUnitOfWork
    {
        public SqliteUnitOfWork(SqliteDbContext context) : base(context) { }
    }

    /// <summary>
    /// Unit of Work implementation dedicated to the SQL Server database.
    /// Use when working with <see cref="SqlServerDbContext"/> repositories.
    /// </summary>
    public class SqlServerUnitOfWork : UnitOfWork<SqlServerDbContext>, ISqlServerUnitOfWork
    {
        public SqlServerUnitOfWork(SqlServerDbContext context) : base(context) { }
    }
}